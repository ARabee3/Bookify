using Bookify.DTOs;
using Bookify.DTOs.PdfProcessor;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Bookify.Interfaces
{
    public interface IBookProcessingService
    {
        Task<BookDetailDto?> ProcessUploadedBookAsync(IFormFile file, string userId);
        Task<ChapterQuizDto?> GenerateQuizForChapterAsync(int chapterId);
    }
}