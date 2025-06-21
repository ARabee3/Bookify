using Bookify.DTOs.Ai;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookify.Interfaces
{
    public interface IAiRecommendationService
    {
        Task<List<AiBookDto>?> GetRankBasedRecommendationsAsync(float? weightViews = null, float? weightRating = null, int? topN = null);
        Task<List<AiBookDto>?> GetFilteredBooksFromAiAsync(FilterCriteriaDto? criteria); // اسم مميز عشان ميختلطش بالفلترة من الداتا بيز بتاعتنا
        Task<ContentRecommendationResponseDto?> GetContentBasedRecommendationsAsync(string bookTitle, int? topN = null);
        // (اختياري) Task<List<AiBookDto>?> GetAiAllBooksAsync(); // لو عايز تجيب كل الكتب من الـ AI API
        // (اختياري) Task<bool> IsAiApiHealthyAsync(); // للـ Health Check
    }
}