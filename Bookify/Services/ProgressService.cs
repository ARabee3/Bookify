using Bookify.Contexts;
using Bookify.DTOs;
using Bookify.Entities;
using Bookify.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore; // للـ AnyAsync
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bookify.Services
{
    public class ProgressService : IProgressService
    {
        private readonly IProgressRepository _progressRepository;
        private readonly IBookRepository _bookRepository;
        // private readonly IChapterRepository _chapterRepository; // <<< تم الحذف
        private readonly AppDbContext _context;
        private readonly IStreakService _streakService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProgressService(
            IProgressRepository progressRepository,
            IBookRepository bookRepository,
            // IChapterRepository chapterRepository, // <<< تم الحذف
            AppDbContext context,
            IStreakService streakService,
            IHttpContextAccessor httpContextAccessor)
        {
            _progressRepository = progressRepository;
            _bookRepository = bookRepository;
            // _chapterRepository = chapterRepository; // <<< تم الحذف
            _context = context;
            _streakService = streakService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ProgressDto?> UpdateOrCreateUserProgressAsync(string userId, UpdateProgressDto progressDto)
        {
            var book = await _bookRepository.GetByIdWithDetailsAsync(progressDto.BookID);
            if (book == null)
            {
                throw new KeyNotFoundException($"Book with ID {progressDto.BookID} not found.");
            }
            if (!book.TotalPages.HasValue || book.TotalPages.Value <= 0)
            {
                // لا يمكن حساب النسبة بدون إجمالي الصفحات، يمكن الاعتماد على النسبة المرسلة فقط
                // أو رمي خطأ إذا كان تتبع الصفحات مطلوباً
                // For now, we'll proceed if CompletionPercentage is provided.
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
                    LastUpdatedAt = DateTime.UtcNow,
                    CompletionPercentage = 0
                };
            }

            bool progressActuallyChanged = false;

            // 1. تحديث LastReadPageNumber (إذا تم إرساله وتغير)
            if (progressDto.LastReadPageNumber.HasValue && existingProgress.LastReadPageNumber != progressDto.LastReadPageNumber.Value)
            {
                existingProgress.LastReadPageNumber = progressDto.LastReadPageNumber.Value;
                progressActuallyChanged = true;
            }

            // 2. تحديد CompletionPercentage
            float newCalculatedPercentage = existingProgress.CompletionPercentage;

            if (progressDto.CompletionPercentage.HasValue)
            {
                newCalculatedPercentage = Math.Clamp(progressDto.CompletionPercentage.Value, 0, 100);
            }
            else if (progressDto.LastReadPageNumber.HasValue && book.TotalPages.HasValue && book.TotalPages.Value > 0)
            {
                newCalculatedPercentage = Math.Clamp(((float)progressDto.LastReadPageNumber.Value / book.TotalPages.Value) * 100, 0, 100);
            }

            if (Math.Abs(existingProgress.CompletionPercentage - newCalculatedPercentage) > 0.01f)
            {
                existingProgress.CompletionPercentage = newCalculatedPercentage;
                progressActuallyChanged = true;
            }

            if (progressActuallyChanged || isNewProgress)
            {
                if (existingProgress.CompletionPercentage >= 99.9f)
                {
                    existingProgress.Status = CompletionStatus.Completed;
                    existingProgress.EndDate = existingProgress.EndDate ?? DateTime.UtcNow;
                }
                // نعتبره InProgress إذا كانت النسبة > 0 أو تم تحديد صفحة (ما لم يكن مكتمل)
                else if (existingProgress.CompletionPercentage > 0 || progressDto.LastReadPageNumber.HasValue)
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
                await _progressRepository.AddAsync(existingProgress);
            }

            bool shouldLogActivity = (progressActuallyChanged && (existingProgress.Status == CompletionStatus.InProgress || existingProgress.Status == CompletionStatus.Completed)) ||
                                     (isNewProgress && (existingProgress.Status == CompletionStatus.InProgress || existingProgress.Status == CompletionStatus.Completed));

            if (shouldLogActivity)
            {
                await LogUserActivityAsync(userId);
                await _streakService.UpdateStreakAsync(userId, DateTime.UtcNow.Date);
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
            var progresses = await _progressRepository.GetCurrentlyReadingProgressAsync(userId);
            var request = _httpContextAccessor.HttpContext?.Request;
            string? baseUrl = null;
            if (request != null) baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";

            return progresses.Select(p => new BookProgressDto
            {
                BookID = p.BookID,
                Title = p.Book?.Title,
                Author = p.Book?.Author,
                Category = p.Book?.Category,
                CoverImageUrl = !string.IsNullOrEmpty(p.Book?.CoverImagePath) && baseUrl != null
                                ? $"{baseUrl}{p.Book.CoverImagePath}"
                                : null,
                CompletionPercentage = p.CompletionPercentage,
                LastReadPageNumber = p.LastReadPageNumber, // <<< تمت الإضافة
                TotalPages = p.Book?.TotalPages,
                LastUpdatedAt = p.LastUpdatedAt
            });
        }

        private async Task LogUserActivityAsync(string userId)
        {
            var today = DateTime.UtcNow.Date;
            bool activityLoggedToday = await _context.UserDailyActivityLogs
                                            .AnyAsync(log => log.UserID == userId && log.ActivityDate == today);
            if (!activityLoggedToday)
            {
                var newLog = new UserDailyActivityLog { UserID = userId, ActivityDate = today };
                await _context.UserDailyActivityLogs.AddAsync(newLog);
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
                BookTitle = progress.Book?.Title,
                BookCoverImageUrl = !string.IsNullOrEmpty(progress.Book?.CoverImagePath) && baseUrl != null
                                    ? $"{baseUrl}{progress.Book.CoverImagePath}"
                                    : null,
                BookPdfFileUrl = !string.IsNullOrEmpty(progress.Book?.PdfFilePath) && baseUrl != null
                                    ? $"{baseUrl}{progress.Book.PdfFilePath}"
                                    : null,
                LastReadPageNumber = progress.LastReadPageNumber, // <<< تمت الإضافة
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