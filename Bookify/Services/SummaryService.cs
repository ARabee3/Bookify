using Bookify.Entities;
using Bookify.Interfaces;
using Bookify.Contexts; // عشان SaveChangesAsync
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // عشان AnyAsync

namespace Bookify.Services
{
    public class SummaryService : ISummaryService
    {
        private readonly ISummaryRepository _summaryRepository;
        private readonly IBookRepository _bookRepository; // نحقن ده كمان عشان نتأكد الكتاب موجود
        //private readonly IChapterRepository _chapterRepository; // لو عايزين نتأكد الشابتر موجود
        private readonly AppDbContext _context; // نحقن ده عشان SaveChanges

        // نعدل الـ Constructor ليستقبل كل الـ Dependencies
        public SummaryService(
            ISummaryRepository summaryRepository,
            IBookRepository bookRepository, // نضيفه هنا
                                            // IChapterRepository chapterRepository, // نضيفه لو عملناه
            AppDbContext context)
        {
            _summaryRepository = summaryRepository;
            _bookRepository = bookRepository;
            // _chapterRepository = chapterRepository;
            _context = context;
        }

        public async Task<IEnumerable<Summary>> GetChapterSummariesForBookAsync(int bookId)
        {
            // ممكن نضيف Check هنا لو الكتاب موجود باستخدام _bookRepository
            var bookExists = await _bookRepository.GetByIdAsync(bookId); // نستخدم الريبو
            if (bookExists == null)
            {
                // ممكن نرجع قايمة فاضية أو نرمي Exception مخصص
                return new List<Summary>(); // أو throw new NotFoundException($"Book with ID {bookId} not found.");
            }
            return await _summaryRepository.GetSummariesByBookIdAsync(bookId);
        }

        public async Task<Summary?> GetSummaryForChapterAsync(int chapterId)
        {
            // ممكن نتأكد إن الشابتر موجود لو عملنا IChapterRepository
            // var chapterExists = await _chapterRepository.ExistsAsync(chapterId);
            // if (!chapterExists) return null;

            return await _summaryRepository.GetSummaryByChapterIdAsync(chapterId);
        }

        public async Task<Summary> AddBookSummaryAsync(int bookId, string content, string userId)
        {
            // 1. التحقق من وجود الكتاب
            var bookExists = await _bookRepository.GetByIdAsync(bookId);
            if (bookExists == null)
            {
                throw new ArgumentException($"Book with ID {bookId} not found."); // نرمي Exception أو نرجع null/result object
            }

            // 2. (اختياري) التحقق من الـ UserID لو ضروري

            // 3. إنشاء كائن الملخص
            var summary = new Summary
            {
                BookID = bookId,
                ChapterID = null,
                UserID = userId,
                Content = content,
                Source = "User Submitted",
                CreateDate = DateTime.UtcNow
            };

            // 4. إضافة الملخص للداتا بيز
            await _summaryRepository.AddAsync(summary);

            // 5. حفظ التغييرات ***هنا*** في الـ Service Layer
            await _context.SaveChangesAsync();

            // 6. نرجع الملخص اللي اتعمل (ممكن يكون الـ ID اتملى)
            return summary;
        }
    }
}