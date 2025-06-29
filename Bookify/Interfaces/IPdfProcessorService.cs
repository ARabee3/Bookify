using Bookify.DTOs.PdfProcessor;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Bookify.Interfaces
{
    public interface IPdfProcessorService
    {
        Task<PdfGetChaptersResponseDto?> GetChaptersAsync(IFormFile pdfFile);
        Task<PdfSummaryResponseDto?> SummarizeChapterAsync(IFormFile pdfFile, int chapterNumber);
        Task<PdfQuizResponseDto?> GenerateQuizForChapterAsync(IFormFile pdfFile, int chapterNumber);
    }
}