using Bookify.Contexts;
using Bookify.DTOs;
using Bookify.DTOs.Ai; // عشان AiSummaryResponseDto
using Bookify.Entities;
using Bookify.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // عشان FirstOrDefaultAsync

namespace Bookify.Services
{
    public class SummaryService : ISummaryService
    {
        private readonly ISummaryRepository _summaryRepository;
        private readonly IChapterRepository _chapterRepository;
        private readonly IAiContentService _aiContentService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly AppDbContext _context;

        public SummaryService(
            ISummaryRepository summaryRepository,
            IChapterRepository chapterRepository,
            IAiContentService aiContentService,
            IWebHostEnvironment webHostEnvironment,
            AppDbContext context)
        {
            _summaryRepository = summaryRepository;
            _chapterRepository = chapterRepository;
            _aiContentService = aiContentService;
            _webHostEnvironment = webHostEnvironment;
            _context = context;
        }

        public async Task<SummaryDto?> GenerateAndSaveSummaryForChapterAsync(int chapterId)
        {
            var chapter = await _chapterRepository.GetByIdAsync(chapterId); // يفترض أن GetByIdAsync يجلب معه الكتاب
            if (chapter == null || chapter.Book == null || string.IsNullOrEmpty(chapter.Book.PdfFilePath))
            {
                throw new KeyNotFoundException($"Chapter with ID {chapterId} or its associated book/PDF path not found.");
            }

            string wwwRootPath = _webHostEnvironment.WebRootPath;
            if (string.IsNullOrEmpty(wwwRootPath)) throw new InvalidOperationException("wwwRoot path is not available.");

            string fullPdfPath = Path.Combine(wwwRootPath, chapter.Book.PdfFilePath.TrimStart('/'));
            if (!File.Exists(fullPdfPath))
            {
                throw new FileNotFoundException("The book's PDF file was not found on the server.", fullPdfPath);
            }

            IFormFile pdfFile;
            using (var stream = new FileStream(fullPdfPath, FileMode.Open, FileAccess.Read))
            {
                pdfFile = new FormFile(stream, 0, stream.Length, "pdf", Path.GetFileName(fullPdfPath))
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "application/pdf"
                };

                var summaryResponse = await _aiContentService.GenerateSummaryAsync(pdfFile, chapter.ChapterNumber);
                if (summaryResponse == null || summaryResponse.Status != "success" || string.IsNullOrEmpty(summaryResponse.Data?.SummaryText))
                {
                    throw new Exception("Failed to generate summary from AI service.");
                }

                var existingSummary = await _context.Summaries.FirstOrDefaultAsync(s => s.ChapterID == chapterId);
                if (existingSummary != null)
                {
                    existingSummary.Content = summaryResponse.Data.SummaryText;
                    existingSummary.CreateDate = DateTime.UtcNow;
                }
                else
                {
                    var newSummary = new Summary
                    {
                        ChapterID = chapterId,
                        BookID = chapter.BookID,
                        Content = summaryResponse.Data.SummaryText,
                        Source = "AI Generated",
                        CreateDate = DateTime.UtcNow
                    };
                    await _summaryRepository.AddAsync(newSummary);
                }

                await _context.SaveChangesAsync();
                var finalSummary = await _context.Summaries.AsNoTracking().FirstOrDefaultAsync(s => s.ChapterID == chapterId);
                return MapToSummaryDto(finalSummary, chapter.Book, chapter);
            }
        }

        public async Task<IEnumerable<SummaryDto>> GetSummariesForBookAsync(int bookId) // <<< تم تعديل الاسم
        {
            var summaries = await _summaryRepository.GetSummariesForBookAsync(bookId); // <<< نفترض وجود هذه الميثود
            return summaries.Select(s => MapToSummaryDto(s, s.Book, s.Chapter)).Where(dto => dto != null).Select(dto => dto!);
        }

        public async Task<SummaryDto?> GetSummaryForChapterAsync(int chapterId)
        {
            var summary = await _summaryRepository.GetSummaryForChapterAsync(chapterId); // <<< نفترض وجود هذه الميثود
            if (summary == null) return null;
            return MapToSummaryDto(summary, summary.Book, summary.Chapter);
        }

        private SummaryDto? MapToSummaryDto(Summary? summary, Book? book, Chapter? chapter)
        {
            if (summary == null) return null;
            return new SummaryDto
            {
                SummaryID = summary.SummaryID,
                BookID = summary.BookID,
                BookTitle = book?.Title,
                ChapterID = summary.ChapterID,
                ChapterTitle = chapter?.Title,
                Content = summary.Content,
                CreateDate = summary.CreateDate, // <<< الـ CreateDate في DTO و Entity كلاهما DateTime
                Source = summary.Source
            };
        }
    }
}