using Bookify.Contexts;
using Bookify.DTOs;
using Bookify.Entities;
using Bookify.Interfaces;
using Microsoft.AspNetCore.Http;    // عشان IHttpContextAccessor
using Microsoft.EntityFrameworkCore; // عشان AnyAsync, FindAsync لو الـ Repository مبيعملش كل حاجة
using System;
using System.Collections.Generic;
using System.Linq;                  // عشان Select, Any, FirstOrDefault
using System.Threading.Tasks;

namespace Bookify.Services
{
    public class ProgressService : IProgressService
    {
        private readonly IProgressRepository _progressRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IChapterRepository _chapterRepository; // تأكد إنك عملت Inject لدي
        private readonly AppDbContext _context; // للـ UserDailyActivityLogs و SaveChanges
        // private readonly IStreakService _streakService; // معلق مؤقتاً
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProgressService(
            IProgressRepository progressRepository,
            IBookRepository bookRepository,
            IChapterRepository chapterRepository, // تأكد من إضافتها هنا
            AppDbContext context,
            // IStreakService streakService, // معلق مؤقتاً
            IHttpContextAccessor httpContextAccessor)
        {
            _progressRepository = progressRepository;
            _bookRepository = bookRepository;
            _chapterRepository = chapterRepository; // تأكد من إضافتها هنا
            _context = context;
            // _streakService = streakService; // معلق مؤقتاً
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ProgressDto?> UpdateOrCreateUserProgressAsync(string userId, UpdateProgressDto progressDto)
        {
            var book = await _bookRepository.GetByIdAsync(progressDto.BookID); // يفترض أن هذا يجلب الكتاب مع Chapters
            if (book == null)
            {
                throw new KeyNotFoundException($"Book with ID {progressDto.BookID} not found.");
            }

            Chapter? chapterEntity = null;
            if (progressDto.LastReadChapterID.HasValue)
            {
                chapterEntity = await _chapterRepository.GetByIdAsync(progressDto.LastReadChapterID.Value);
                if (chapterEntity == null || chapterEntity.BookID != progressDto.BookID)
                {
                    throw new ArgumentException($"Chapter with ID {progressDto.LastReadChapterID.Value} not found or does not belong to book ID {progressDto.BookID}.");
                }
            }

            var existingProgress = await _progressRepository.GetUserProgressForBookAsync(userId, progressDto.BookID);
            bool isNewProgress = existingProgress == null;

            if (isNewProgress)
            {
                existingProgress = new Progress
                {
                    UserID = userId,
                    BookID = progressDto.BookID,
                    StartDate = DateTime.UtcNow,
                    Status = CompletionStatus.NotStarted,
                    LastUpdatedAt = DateTime.UtcNow
                };
            }

            bool progressActuallyChanged = false;

            if (progressDto.LastReadChapterID.HasValue && existingProgress.LastReadChapterID != progressDto.LastReadChapterID.Value)
            {
                existingProgress.LastReadChapterID = progressDto.LastReadChapterID.Value;
                progressActuallyChanged = true;
            }

            if (progressDto.CompletionPercentage.HasValue)
            {
                float newPercentage = Math.Clamp(progressDto.CompletionPercentage.Value, 0, 100);
                if (Math.Abs(existingProgress.CompletionPercentage - newPercentage) > 0.01f)
                {
                    existingProgress.CompletionPercentage = newPercentage;
                    progressActuallyChanged = true;
                }
            }
            else if (progressDto.LastReadChapterID.HasValue && chapterEntity != null && book.Chapters != null && book.Chapters.Any())
            {
                float calculatedPercentage = ((float)chapterEntity.ChapterNumber / book.Chapters.Count()) * 100;
                calculatedPercentage = Math.Min(calculatedPercentage, 100);
                if (Math.Abs(existingProgress.CompletionPercentage - calculatedPercentage) > 0.01f)
                {
                    existingProgress.CompletionPercentage = calculatedPercentage;
                    progressActuallyChanged = true;
                }
            }

            if (progressActuallyChanged || isNewProgress)
            {
                if (existingProgress.CompletionPercentage >= 99.9f)
                {
                    existingProgress.Status = CompletionStatus.Completed;
                    existingProgress.EndDate = existingProgress.EndDate ?? DateTime.UtcNow;
                }
                else if (existingProgress.CompletionPercentage > 0 || existingProgress.LastReadChapterID.HasValue)
                {
                    existingProgress.Status = CompletionStatus.InProgress;
                    existingProgress.EndDate = null;
                    if (isNewProgress && existingProgress.StartDate == null) existingProgress.StartDate = DateTime.UtcNow;
                }
                else if (isNewProgress)
                {
                    existingProgress.Status = CompletionStatus.NotStarted;
                }
                existingProgress.LastUpdatedAt = DateTime.UtcNow;
            }

            if (isNewProgress)
            {
                await _progressRepository.AddProgressAsync(existingProgress);
            }
            // EF Core يتتبع التغييرات، Update صريح قد لا يكون مطلوباً إذا الـ Entity متتبعة
            // else if (progressActuallyChanged)
            // {
            //     await _progressRepository.UpdateProgressAsync(existingProgress);
            // }


            // --- تم تعديل الشرط هنا ---
            bool shouldLogActivity = (progressActuallyChanged && (existingProgress.Status == CompletionStatus.InProgress || existingProgress.Status == CompletionStatus.Completed)) ||
                                     (isNewProgress && (existingProgress.Status == CompletionStatus.InProgress || existingProgress.Status == CompletionStatus.Completed));

            if (shouldLogActivity)
            {
                await LogUserActivityAsync(userId);
                // await _streakService.UpdateStreakAsync(userId, DateTime.UtcNow.Date); // معلق مؤقتاً
            }

            await _context.SaveChangesAsync();

            var updatedProgressWithDetails = await _progressRepository.GetUserProgressForBookAsync(userId, progressDto.BookID);
            return MapToProgressDto(updatedProgressWithDetails);
        }

        public async Task<ProgressDto?> GetUserProgressForBookAsync(string userId, int bookId)
        {
            var progress = await _progressRepository.GetUserProgressForBookAsync(userId, bookId);
            return MapToProgressDto(progress);
        }

        public async Task<IEnumerable<ProgressDto>> GetAllUserProgressAsync(string userId)
        {
            var progresses = await _progressRepository.GetAllUserProgressAsync(userId);
            return progresses.Select(p => MapToProgressDto(p)).Where(dto => dto != null).Select(dto => dto!);
        }

        public async Task<IEnumerable<BookProgressDto>> GetCurrentlyReadingBooksAsync(string userId)
        {
            var progresses = await _progressRepository.GetCurrentlyReadingProgressAsync(userId); // استخدام الريبو

            var request = _httpContextAccessor.HttpContext?.Request;
            string? baseUrl = null;
            if (request != null) baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";

            return progresses.Select(p => new BookProgressDto
            {
                BookID = p.BookID,
                Title = p.Book?.Title, // يعتمد على Include في الريبو
                Author = p.Book?.Author,
                Category = p.Book?.Category,
                CoverImageUrl = !string.IsNullOrEmpty(p.Book?.CoverImagePath) && baseUrl != null
                                ? $"{baseUrl}{p.Book.CoverImagePath}"
                                : null,
                CompletionPercentage = p.CompletionPercentage,
                LastReadChapterID = p.LastReadChapterID,
                TotalPages = p.Book?.TotalPages,
                LastUpdatedAt = p.LastUpdatedAt
            });
        }

        private async Task LogUserActivityAsync(string userId)
        {
            var today = DateTime.UtcNow.Date;
            // استخدام _context مباشرة هنا مقبول أو يمكن عمل Repository لـ UserDailyActivityLog
            bool activityLoggedToday = await _context.UserDailyActivityLogs
                                            .AnyAsync(log => log.UserID == userId && log.ActivityDate == today);
            if (!activityLoggedToday)
            {
                var newLog = new UserDailyActivityLog { UserID = userId, ActivityDate = today };
                await _context.UserDailyActivityLogs.AddAsync(newLog);
                // SaveChanges سيتم في الميثود الأساسية
            }
        }

        private ProgressDto? MapToProgressDto(Progress? progress)
        {
            if (progress == null) return null;

            var request = _httpContextAccessor.HttpContext?.Request;
            string? baseUrl = null;
            if (request != null) baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";

            return new ProgressDto
            {
                ProgressID = progress.ProgressID,
                BookID = progress.BookID,
                BookTitle = progress.Book?.Title, // يعتمد على Include في الريبو
                BookCoverImageUrl = !string.IsNullOrEmpty(progress.Book?.CoverImagePath) && baseUrl != null
                                    ? $"{baseUrl}{progress.Book.CoverImagePath}"
                                    : null,
                LastReadChapterID = progress.LastReadChapterID,
                LastReadChapterTitle = progress.LastReadChapter?.Title, // يعتمد على Include في الريبو
                CompletionPercentage = progress.CompletionPercentage,
                Status = progress.Status,
                StartDate = progress.StartDate,
                EndDate = progress.EndDate,
                TotalPagesInBook = progress.Book?.TotalPages,
                LastUpdatedAt = progress.LastUpdatedAt
            };
        }
    }
}