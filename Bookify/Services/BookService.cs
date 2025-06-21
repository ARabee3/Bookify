using Bookify.Contexts;
using Bookify.DTOs;
using Bookify.Entities;
using Bookify.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bookify.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly AppDbContext _context; // لا يزال يستخدم في GetBookByIdAsync لـ Includes وحساب Ratings
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BookService(
            IBookRepository bookRepository,
            AppDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _bookRepository = bookRepository;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<BookListItemDto>> GetAllBooksAsync(string? category)
        {
            IEnumerable<Book> booksFromRepo;
            if (!string.IsNullOrEmpty(category))
            {
                booksFromRepo = await _bookRepository.GetByCategoryAsync(category);
            }
            else
            {
                booksFromRepo = await _bookRepository.GetAllAsync();
            }

            var request = _httpContextAccessor.HttpContext?.Request;
            string? baseUrl = null;
            if (request != null)
            {
                baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
            }

            return booksFromRepo.Select(book => new BookListItemDto
            {
                BookID = book.BookID,
                Title = book.Title,
                Author = book.Author,
                Category = book.Category,
                CoverImageUrl = !string.IsNullOrEmpty(book.CoverImagePath) && baseUrl != null
                                ? $"{baseUrl}{book.CoverImagePath}"
                                : null,
                AverageRating = book.Rating,
                Difficulty = book.Difficulty,
                Views = book.Views,
                Language = book.Language,
                ReleaseYear = book.ReleaseYear,
                Prerequisites = book.Prerequisites,
                LearningObjectives = book.LearningObjectives,
                TotalPages = book.TotalPages
            }).ToList();
        }

        public async Task<BookDetailDto?> GetBookByIdAsync(int id, string? currentUserId = null)
        {
            var bookEntity = await _bookRepository.GetByIdAsync(id); // <<< تم التعديل لاستخدام الريبو مباشرة (بافتراض أن الريبو يعمل Include اللازم)

            if (bookEntity == null)
            {
                return null;
            }

            var request = _httpContextAccessor.HttpContext?.Request;
            string? baseUrl = null;
            if (request != null)
            {
                baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
            }

            float? averageRatingCalc = null;
            int totalRatingsCalc = 0;

            // حساب التقييمات من _context مباشرة أو من bookEntity.Ratings لو الريبو عمل Include
            var ratingsForBookQuery = _context.UserBookRatings.Where(r => r.BookID == id);
            if (await ratingsForBookQuery.AnyAsync())
            {
                averageRatingCalc = await ratingsForBookQuery.AverageAsync(r => r.Rating);
                totalRatingsCalc = await ratingsForBookQuery.CountAsync();
            }


            return new BookDetailDto
            {
                BookID = bookEntity.BookID,
                Title = bookEntity.Title,
                Author = bookEntity.Author,
                Category = bookEntity.Category,
                Source = bookEntity.Source,
                PdfFileUrl = !string.IsNullOrEmpty(bookEntity.PdfFilePath) && baseUrl != null ? $"{baseUrl}{bookEntity.PdfFilePath}" : null,
                CoverImageUrl = !string.IsNullOrEmpty(bookEntity.CoverImagePath) && baseUrl != null ? $"{baseUrl}{bookEntity.CoverImagePath}" : null,
                Description = bookEntity.Summary,
                Difficulty = bookEntity.Difficulty,
                AverageRating = averageRatingCalc ?? bookEntity.Rating, // نستخدم التقييم العام لو مفيش تقييمات
                TotalRatings = totalRatingsCalc,
                Views = bookEntity.Views,
                Language = bookEntity.Language,
                ReleaseYear = bookEntity.ReleaseYear,
                Prerequisites = bookEntity.Prerequisites,
                LearningObjectives = bookEntity.LearningObjectives,
                TotalPages = bookEntity.TotalPages,
                TotalChapters = bookEntity.Chapters?.Count, // Chapters يجب أن تأتي من الـ Include في الريبو
                Chapters = bookEntity.Chapters?.Select(c => new ChapterSummaryDto
                {
                    ChapterID = c.ChapterID,
                    Title = c.Title,
                    ChapterNumber = c.ChapterNumber
                }).ToList() ?? new List<ChapterSummaryDto>()
            };
        }

        // --- تم تعديل هذه الميثود لترجع List<BookListItemDto> ---
        public async Task<List<BookListItemDto>> GetBooksByTitlesAsync(List<string> titles)
        {
            var booksFromRepo = await _bookRepository.GetByTitlesAsync(titles);

            var request = _httpContextAccessor.HttpContext?.Request;
            string? baseUrl = null;
            if (request != null) baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";

            return booksFromRepo.Select(book => new BookListItemDto
            {
                BookID = book.BookID,
                Title = book.Title,
                Author = book.Author,
                Category = book.Category,
                CoverImageUrl = !string.IsNullOrEmpty(book.CoverImagePath) && baseUrl != null
                                ? $"{baseUrl}{book.CoverImagePath}"
                                : null,
                AverageRating = book.Rating,
                Difficulty = book.Difficulty,
                Views = book.Views,
                Language = book.Language,
                ReleaseYear = book.ReleaseYear,
                Prerequisites = book.Prerequisites,
                LearningObjectives = book.LearningObjectives,
                TotalPages = book.TotalPages
            }).ToList();
        }
    }
}