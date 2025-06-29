using Bookify.Contexts;
using Bookify.DTOs;
using Bookify.Entities;
using Bookify.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO; // For Path
using System.Linq;
using System.Threading.Tasks;

namespace Bookify.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IChapterRepository _chapterRepository;
        private readonly ISummaryRepository _summaryRepository;
        private readonly IAiContentService _aiContentService; // For AI integration
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _webHostEnvironment; // For file uploads
        private readonly AppDbContext _context; // For SaveChanges

        public BookService(
            IBookRepository bookRepository,
            IChapterRepository chapterRepository,
            ISummaryRepository summaryRepository,
            IAiContentService aiContentService,
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment webHostEnvironment,
            AppDbContext context)
        {
            _bookRepository = bookRepository;
            _chapterRepository = chapterRepository;
            _summaryRepository = summaryRepository;
            _aiContentService = aiContentService;
            _httpContextAccessor = httpContextAccessor;
            _webHostEnvironment = webHostEnvironment;
            _context = context;
        }

        // --- تم تعديل هذه الميثود بالكامل ---
        public async Task<PaginatedFilteredBooksDto> GetAllBooksAsync(BookFilterDto filter)
        {
            // The repository will handle the actual filtering and pagination
            var paginatedBooksTuple = await _bookRepository.GetAllFilteredAndPaginatedAsync(filter);
            var booksFromRepo = paginatedBooksTuple.books;
            var totalBooksCount = paginatedBooksTuple.totalCount;

            var bookListDtos = MapBooksToListItemDtos(booksFromRepo);

            return new PaginatedFilteredBooksDto
            {
                TotalBooks = totalBooksCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                Books = bookListDtos
            };
        }

        public async Task<BookDetailDto?> GetBookByIdAsync(int id, string? currentUserId = null)
        {
            var bookEntity = await _bookRepository.GetByIdWithDetailsAsync(id);
            if (bookEntity == null) return null;

            // Security Check:
            // Allow access if the book is public OR if the user is the one who uploaded it.
            if (!bookEntity.IsPublic && bookEntity.UploadedBy != currentUserId)
            {
                // User is trying to access a private book they do not own.
                return null;
            }

            return MapBookToDetailDto(bookEntity);
        }

        public async Task<List<BookListItemDto>> GetBooksByTitlesAsync(List<string> titles)
        {
            var booksFromRepo = await _bookRepository.GetByTitlesAsync(titles);
            return MapBooksToListItemDtos(booksFromRepo).ToList();
        }

        public async Task<BookDetailDto?> UploadAndProcessBookAsync(IFormFile file, string userId)
        {
            // 1. حفظ ملف الـ PDF
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            string uploadsFolder = "BookPdfs";
            string targetFolderPath = Path.Combine(wwwRootPath, uploadsFolder);
            if (!Directory.Exists(targetFolderPath)) Directory.CreateDirectory(targetFolderPath);

            string uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            string fullFilePathOnServer = Path.Combine(targetFolderPath, uniqueFileName);

            using (var stream = new FileStream(fullFilePathOnServer, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            string relativePath = $"/{uploadsFolder}/{uniqueFileName}";

            // 2. إنشاء سجل مبدئي للكتاب
            var book = new Book
            {
                Title = Path.GetFileNameWithoutExtension(file.FileName),
                Source = "UserUpload",
                UploadedBy = userId,
                PdfFilePath = relativePath,
                // Status = "Processing" // إذا كان لديك حقل Status
            };
            await _bookRepository.AddAsync(book);
            await _context.SaveChangesAsync();

            // 3. استدعاء الـ AI API لاكتشاف الشابترات
            var chaptersResponse = await _aiContentService.DiscoverChaptersAsync(file);
            if (chaptersResponse == null || chaptersResponse.Status != "success")
            {
                // throw new Exception("Failed to discover chapters from AI service.");
                return null;
            }

            // 4. حفظ الشابترات في قاعدة البيانات
            int chapterNumberCounter = 1;
            var createdChapters = new List<Chapter>();
            foreach (var aiChapter in chaptersResponse.Data.Chapters)
            {
                var newChapter = new Chapter
                {
                    BookID = book.BookID,
                    Title = aiChapter.Title,
                    ChapterNumber = chapterNumberCounter++,
                    // StartPage = aiChapter.StartPage, // إذا أضفت هذه الأعمدة
                    // EndPage = aiChapter.EndPage
                };
                await _chapterRepository.AddAsync(newChapter);
                createdChapters.Add(newChapter); // للاستخدام لاحقاً بدون جلبها من الداتا بيز
            }
            await _context.SaveChangesAsync();

            // 5. توليد وحفظ الملخصات لكل شابتر (يمكن عملها في Background Job لاحقاً)
            foreach (var chapter in createdChapters)
            {
                var summaryResponse = await _aiContentService.GenerateSummaryAsync(file, chapter.ChapterNumber);
                if (summaryResponse != null && summaryResponse.Status == "success")
                {
                    var newSummary = new Summary
                    {
                        ChapterID = chapter.ChapterID,
                        BookID = book.BookID,
                        UserID = userId,
                        Content = summaryResponse.Data.SummaryText,
                        Source = "AI Generated"
                    };
                    await _summaryRepository.AddAsync(newSummary);
                }
            }
            await _context.SaveChangesAsync();
            // TODO: Add quiz generation logic here similarly

            // 6. إرجاع تفاصيل الكتاب الجديد بعد المعالجة
            return await GetBookByIdAsync(book.BookID, userId);
        }

        // --- Helper Method للـ Mapping ---
        private List<BookListItemDto> MapBooksToListItemDtos(IEnumerable<Book> books)
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            string? baseUrl = (request != null) ? $"{request.Scheme}://{request.Host}{request.PathBase}" : null;

            return books.Select(book => new BookListItemDto
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

        private BookDetailDto MapBookToDetailDto(Book bookEntity)
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            string? baseUrl = (request != null) ? $"{request.Scheme}://{request.Host}{request.PathBase}" : null;

            // حساب التقييمات (هذا قد يسبب N+1 إذا لم يتم عمل Include للـ Ratings في الريبو)
            float? averageRatingCalc = bookEntity.Ratings?.Any() == true ? bookEntity.Ratings.Average(r => r.Rating) : (float?)null;
            int totalRatingsCalc = bookEntity.Ratings?.Count() ?? 0;

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
                AverageRating = averageRatingCalc ?? bookEntity.Rating,
                TotalRatings = totalRatingsCalc,
                Views = bookEntity.Views,
                Language = bookEntity.Language,
                ReleaseYear = bookEntity.ReleaseYear,
                Prerequisites = bookEntity.Prerequisites,
                LearningObjectives = bookEntity.LearningObjectives,
                TotalPages = bookEntity.TotalPages,
                TotalChapters = bookEntity.Chapters?.Count,
                Chapters = bookEntity.Chapters?.Select(c => new ChapterSummaryDto
                {
                    ChapterID = c.ChapterID,
                    Title = c.Title,
                    ChapterNumber = c.ChapterNumber
                }).ToList() ?? new List<ChapterSummaryDto>()
            };
        }
    }
}