using Bookify.Contexts; // لـ SaveChangesAsync
using Bookify.DTOs;
using Bookify.Entities;
using Bookify.Interfaces;
using Microsoft.AspNetCore.Http; // لـ IHttpContextAccessor
using Microsoft.EntityFrameworkCore; // لـ AnyAsync
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bookify.Services
{
    public class UserLibraryService : IUserLibraryService
    {
        private readonly IUserLibraryRepository _userLibraryRepository;
        private readonly IBookRepository _bookRepository; // للتحقق من وجود الكتاب
        private readonly AppDbContext _context; // لـ SaveChangesAsync
        private readonly IHttpContextAccessor _httpContextAccessor; // لبناء الـ URLs للصور

        public UserLibraryService(
            IUserLibraryRepository userLibraryRepository,
            IBookRepository bookRepository,
            AppDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _userLibraryRepository = userLibraryRepository;
            _bookRepository = bookRepository;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> AddBookToMyLibraryAsync(string userId, int bookId)
        {
            var bookExists = await _bookRepository.GetByIdAsync(bookId);
            if (bookExists == null)
            {
                return false; // الكتاب غير موجود أصلاً
            }

            var alreadyInLibrary = await _userLibraryRepository.IsBookInUserLibraryAsync(userId, bookId);
            if (alreadyInLibrary)
            {
                return true; // أو false مع رسالة "موجود بالفعل" - حسب ما يفضل الـ Frontend
            }

            var userLibraryBook = new UserLibraryBook
            {
                UserID = userId,
                BookID = bookId,
                AddedAt = DateTime.UtcNow
            };

            await _userLibraryRepository.AddBookAsync(userLibraryBook);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveBookFromMyLibraryAsync(string userId, int bookId)
        {
            var bookInLibrary = await _userLibraryRepository.GetUserLibraryBookAsync(userId, bookId);
            if (bookInLibrary == null)
            {
                return false; // الكتاب ليس في مكتبة المستخدم أصلاً
            }

            await _userLibraryRepository.RemoveBookAsync(bookInLibrary);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<BookListItemDto>> GetMyLibraryBooksAsync(string userId, int pageNumber, int pageSize)
        {
            var booksFromRepo = await _userLibraryRepository.GetUserLibraryBooksAsync(userId);

            // تطبيق الـ Pagination هنا بعد جلب كل الكتب من الريبو
            // (أو يمكن تعديل الريبو ليقوم بالـ Pagination إذا كان العدد كبيراً جداً)
            var pagedBooks = booksFromRepo
                                .Skip((pageNumber - 1) * pageSize)
                                .Take(pageSize);

            var request = _httpContextAccessor.HttpContext?.Request;
            string? baseUrl = null;
            if (request != null)
            {
                baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
            }

            return pagedBooks.Select(book => new BookListItemDto
            {
                BookID = book.BookID,
                Title = book.Title,
                Author = book.Author,
                Category = book.Category,
                CoverImageUrl = !string.IsNullOrEmpty(book.CoverImagePath) && baseUrl != null
                                ? $"{baseUrl}{book.CoverImagePath}"
                                : null,
                AverageRating = book.Rating, // نفترض أن Rating في Book هو المتوسط
                Difficulty = book.Difficulty,
                Views = book.Views,
                Language = book.Language,
                ReleaseYear = book.ReleaseYear,
                Prerequisites = book.Prerequisites,
                LearningObjectives = book.LearningObjectives,
                TotalPages = book.TotalPages
            }).ToList();
        }

        public async Task<bool> CheckIfBookInMyLibraryAsync(string userId, int bookId)
        {
            return await _userLibraryRepository.IsBookInUserLibraryAsync(userId, bookId);
        }
    }
}