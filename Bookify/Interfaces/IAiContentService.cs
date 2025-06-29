using Bookify.DTOs.Ai; // هنعمل الـ DTOs دي
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Bookify.Interfaces
{
    public interface IAiContentService
    {
        Task<AiGetChaptersResponseDto?> DiscoverChaptersAsync(IFormFile pdfFile);
        Task<AiQuizResponseDto?> GenerateQuizAsync(IFormFile pdfFile, int chapterNumber);
        Task<AiSummaryResponseDto?> GenerateSummaryAsync(IFormFile pdfFile, int chapterNumber);
    }
}