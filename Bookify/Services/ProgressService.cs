using Bookify.Contexts;
using Bookify.DTOs;
using Bookify.Entities;
using Bookify.Interfaces; // <<< تأكد من وجود using دي
using Microsoft.EntityFrameworkCore;
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
        private readonly AppDbContext _context;
        private readonly IStreakService _streakService; // <<< تم إضافته

        public ProgressService(
            IProgressRepository progressRepository,
            IBookRepository bookRepository,
            AppDbContext context,
            IStreakService streakService) // <<< تم إضافته
        {
            _progressRepository = progressRepository;
            _bookRepository = bookRepository;
            _context = context;
            _streakService = streakService; // <<< تم إضافته
        }

        public async Task<ProgressDto?> UpdateOrCreateUserProgressAsync(string userId, UpdateProgressDto progressDto)
        {
            var book = await _bookRepository.GetByIdAsync(progressDto.BookID);
            if (book == null)
            {
                throw new KeyNotFoundException($"Book with ID {progressDto.BookID} not found. Cannot update progress.");
            }

            if (progressDto.LastReadChapterID.HasValue)
            {
                var chapterExists = await _context.Chapters.AnyAsync(c => c.ChapterID == progressDto.LastReadChapterID.Value && c.BookID == progressDto.BookID);
                if (!chapterExists)
                {
                    throw new ArgumentException($"Chapter with ID {progressDto.LastReadChapterID.Value} does not belong to book with ID {progressDto.BookID}.");
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
                    Status = CompletionStatus.NotStarted
                };
            }

            bool progressChanged = false;

            if (progressDto.LastReadChapterID.HasValue && existingProgress.LastReadChapterID != progressDto.LastReadChapterID.Value)
            {
                existingProgress.LastReadChapterID = progressDto.LastReadChapterID.Value;
                progressChanged = true;
            }

            if (progressDto.CompletionPercentage.HasValue && existingProgress.CompletionPercentage != progressDto.CompletionPercentage.Value)
            {
                existingProgress.CompletionPercentage = progressDto.CompletionPercentage.Value;
                progressChanged = true;
            }

            if (progressChanged)
            {
                if (existingProgress.CompletionPercentage >= 100)
                {
                    existingProgress.Status = CompletionStatus.Completed;
                    existingProgress.EndDate = DateTime.UtcNow;
                }
                else if (existingProgress.CompletionPercentage > 0 || existingProgress.LastReadChapterID.HasValue)
                {
                    existingProgress.Status = CompletionStatus.InProgress;
                    existingProgress.EndDate = null;
                }
                else
                {
                    existingProgress.Status = CompletionStatus.NotStarted;
                    existingProgress.EndDate = null;
                }
            }

            if (isNewProgress)
            {
                await _progressRepository.AddProgressAsync(existingProgress);
            }
            else if (progressChanged)
            {
                await _progressRepository.UpdateProgressAsync(existingProgress);
            }

            if (progressChanged || isNewProgress)
            {
                await LogUserActivityAsync(userId);
                // --- تحديث الـ Streak بعد تسجيل النشاط ---
                await _streakService.UpdateStreakAsync(userId, DateTime.UtcNow.Date);
                // ---------------------------------------
            }

            if (progressChanged || isNewProgress)
            {
                await _context.SaveChangesAsync();
            }

            // لتحسين الأداء، الأفضل تجنب استدعاء الداتا بيز تاني هنا لو ممكن
            // ممكن نمرر عنوان الشابتر لو جبناه قبل كده
            string? lastChapterTitle = null;
            if (existingProgress.LastReadChapterID.HasValue)
            {
                var lastChapter = await _context.Chapters.FindAsync(existingProgress.LastReadChapterID.Value);
                lastChapterTitle = lastChapter?.Title;
            }
            return MapToProgressDto(existingProgress, book.Title, lastChapterTitle);
        }

        public async Task<ProgressDto?> GetUserProgressForBookAsync(string userId, int bookId)
        {
            var progress = await _progressRepository.GetUserProgressForBookAsync(userId, bookId);
            if (progress == null) return null;

            var book = await _bookRepository.GetByIdAsync(bookId);
            string? chapterTitle = null;
            if (progress.LastReadChapterID.HasValue)
            {
                var chapter = await _context.Chapters.FindAsync(progress.LastReadChapterID.Value);
                chapterTitle = chapter?.Title;
            }
            return MapToProgressDto(progress, book?.Title, chapterTitle);
        }

        public async Task<IEnumerable<ProgressDto>> GetAllUserProgressAsync(string userId)
        {
            var progresses = await _progressRepository.GetAllUserProgressAsync(userId);
            var progressDtos = new List<ProgressDto>();
            foreach (var p in progresses)
            {
                string? chapterTitle = null;
                if (p.LastReadChapterID.HasValue)
                {
                    var chapter = await _context.Chapters.FindAsync(p.LastReadChapterID.Value);
                    chapterTitle = chapter?.Title;
                }
                progressDtos.Add(MapToProgressDto(p, p.Book?.Title, chapterTitle));
            }
            return progressDtos;
        }

        public async Task<IEnumerable<BookProgressDto>> GetCurrentlyReadingBooksAsync(string userId)
        {
            var progresses = await _context.Progresses
                .Where(p => p.UserID == userId && p.Status == CompletionStatus.InProgress)
                .Include(p => p.Book)
                // .ThenInclude(b => b.Chapters) // مش محتاجين كل الشابترات هنا
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();

            return progresses.Select(p => new BookProgressDto
            {
                BookID = p.BookID,
                Title = p.Book?.Title,
                Author = p.Book?.Author,
                Category = p.Book?.Category,
                PdfFilePath = p.Book?.PdfFilePath,
                CompletionPercentage = p.CompletionPercentage,
                LastReadChapterID = p.LastReadChapterID,
                TotalPages = p.Book?.TotalPages
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

        private ProgressDto? MapToProgressDto(Progress? progress, string? bookTitle, string? chapterTitle = null)
        {
            if (progress == null) return null;
            return new ProgressDto
            {
                ProgressID = progress.ProgressID,
                BookID = progress.BookID,
                BookTitle = bookTitle,
                LastReadChapterID = progress.LastReadChapterID,
                LastReadChapterTitle = chapterTitle,
                CompletionPercentage = progress.CompletionPercentage,
                Status = progress.Status,
                StartDate = progress.StartDate,
                EndDate = progress.EndDate
            };
        }
    }
}