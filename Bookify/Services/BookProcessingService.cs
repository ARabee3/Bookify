using Bookify.Contexts;
using Bookify.DTOs;
using Bookify.DTOs.PdfProcessor;
using Bookify.Entities;
using Bookify.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Bookify.Services
{
    public class BookProcessingService : IBookProcessingService
    {
        private readonly AppDbContext _context;
        private readonly IPdfProcessorService _pdfProcessorService;
        private readonly IBookService _bookService; // To get final DTO
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<BookProcessingService> _logger;

        public BookProcessingService(
            AppDbContext context,
            IPdfProcessorService pdfProcessorService,
            IBookService bookService,
            IWebHostEnvironment webHostEnvironment,
            ILogger<BookProcessingService> logger)
        {
            _context = context;
            _pdfProcessorService = pdfProcessorService;
            _bookService = bookService;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public async Task<BookDetailDto?> ProcessUploadedBookAsync(IFormFile file, string userId)
        {
            // 1. Save the PDF file
            var (relativePath, _) = await SaveFileAsync(file, "BookPdfs");
            if (string.IsNullOrEmpty(relativePath))
            {
                throw new InvalidOperationException("Failed to save the uploaded PDF file.");
            }

            // 2. Create the initial Book record
            var book = new Book
            {
                Title = Path.GetFileNameWithoutExtension(file.FileName),
                Source = "UserUpload_V2",
                UploadedBy = userId,
                PdfFilePath = relativePath,
                IsPublic = false
            };
            _context.Books.Add(book);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Created private book record with ID: {BookID} for user {UserId}", book.BookID, userId);

            // 3. Add the book to the user's library
            var userLibraryBook = new UserLibraryBook { UserID = userId, BookID = book.BookID };
            _context.UserLibraryBooks.Add(userLibraryBook);

            // 4. Get Chapters from AI Service (This is the only long part now, ~1-3 mins)
            var chaptersData = await _pdfProcessorService.GetChaptersAsync(file);
            if (chaptersData == null || !chaptersData.Chapters.Any())
            {
                _logger.LogWarning("AI Service could not identify chapters for book ID {BookID}.", book.BookID);
            }
            else
            {
                // 5. Save Chapters to our Database
                int chapterCounter = 1;
                foreach (var aiChapter in chaptersData.Chapters)
                {
                    var newChapter = new Chapter
                    {
                        BookID = book.BookID,
                        Title = aiChapter.Title,
                        ChapterNumber = chapterCounter++,
                        StartPage = aiChapter.StartPage,
                        EndPage = aiChapter.EndPage
                    };
                    _context.Chapters.Add(newChapter);
                }
                _logger.LogInformation("Saved {ChapterCount} chapters for book ID: {BookID}", chaptersData.Chapters.Count, book.BookID);
            }

            // --- THE SUMMARY GENERATION LOOP HAS BEEN REMOVED ---

            await _context.SaveChangesAsync();

            // 6. Return the fully processed book details
            return await _bookService.GetBookByIdAsync(book.BookID, userId);
        }

        public async Task<ChapterQuizDto?> GenerateQuizForChapterAsync(int chapterId)
        {
            var chapter = await _context.Chapters
                                .Include(c => c.Book)
                                .FirstOrDefaultAsync(c => c.ChapterID == chapterId);

            if (chapter == null) throw new KeyNotFoundException("Chapter not found.");
            if (chapter.Book == null || string.IsNullOrEmpty(chapter.Book.PdfFilePath))
            {
                throw new InvalidOperationException("Chapter is not associated with a valid book PDF.");
            }

            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, chapter.Book.PdfFilePath.TrimStart('/'));
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("The book's PDF file could not be found on the server.", fullPath);
            }

            IFormFile pdfFile;
            await using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
            {
                pdfFile = new FormFile(stream, 0, stream.Length, "file", Path.GetFileName(fullPath));
                var quizData = await _pdfProcessorService.GenerateQuizForChapterAsync(pdfFile, chapter.ChapterNumber);

                if (quizData == null) return null;

                // Map to our clean DTO for the frontend
                return new ChapterQuizDto
                {
                    QuizTitle = quizData.QuizTitle,
                    Questions = quizData.Questions
                };
            }
        }

        private async Task<(string?, string?)> SaveFileAsync(IFormFile file, string subfolder)
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            if (string.IsNullOrEmpty(wwwRootPath)) return (null, null);

            string targetFolderPath = Path.Combine(wwwRootPath, subfolder);
            if (!Directory.Exists(targetFolderPath)) Directory.CreateDirectory(targetFolderPath);

            string uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            string fullFilePath = Path.Combine(targetFolderPath, uniqueFileName);

            using (var stream = new FileStream(fullFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            string relativePath = $"/{subfolder}/{uniqueFileName}";
            return (relativePath, fullFilePath);
        }
    }
}