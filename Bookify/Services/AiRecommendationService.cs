using Bookify.DTOs.Ai;
using Bookify.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json; // تأكد من وجود باكج System.Net.Http.Json
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Services
{
    public class AiRecommendationService : IAiRecommendationService
    {
        private readonly HttpClient _httpClient;
        public AiRecommendationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<AiBookDto>?> GetRankBasedRecommendationsAsync(float? weightViews = null, float? weightRating = null, int? topN = null)
        {
            var queryStringBuilder = new StringBuilder("recommendations/rank"); // المسار النسبي
            bool firstParam = true;

            Action<string, string?> appendQueryParam = (name, value) =>
            {
                if (value != null)
                {
                    queryStringBuilder.Append(firstParam ? "?" : "&");
                    queryStringBuilder.Append($"{name}={Uri.EscapeDataString(value)}");
                    firstParam = false;
                }
            };

            appendQueryParam("weight_views", weightViews?.ToString(System.Globalization.CultureInfo.InvariantCulture));
            appendQueryParam("weight_rating", weightRating?.ToString(System.Globalization.CultureInfo.InvariantCulture));
            appendQueryParam("top_n", topN?.ToString());

            try
            {
                return await _httpClient.GetFromJsonAsync<List<AiBookDto>>(queryStringBuilder.ToString());
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"AI API Error (Rank): {ex.StatusCode} - {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception calling AI API (Rank): {ex.Message}");
                return null;
            }
        }

        public async Task<List<AiBookDto>?> GetFilteredBooksFromAiAsync(FilterCriteriaDto? criteria)
        {
            if (criteria == null) return new List<AiBookDto>();

            var queryStringBuilder = new StringBuilder("recommendations/filter");
            bool firstParam = true;
            Action<string, string?> appendQueryParam = (name, value) => {
                if (value != null)
                {
                    queryStringBuilder.Append(firstParam ? "?" : "&");
                    queryStringBuilder.Append($"{name}={Uri.EscapeDataString(value)}");
                    firstParam = false;
                }
            };

            appendQueryParam("category", criteria.Category);
            appendQueryParam("difficulty", criteria.Difficulty);
            appendQueryParam("language", criteria.Language);
            appendQueryParam("min_views", criteria.MinViews?.ToString());
            appendQueryParam("min_rating", criteria.MinRating?.ToString(System.Globalization.CultureInfo.InvariantCulture));
            appendQueryParam("author", criteria.Author);
            appendQueryParam("recent_years", criteria.RecentYears?.ToString());

            try
            {
                return await _httpClient.GetFromJsonAsync<List<AiBookDto>>(queryStringBuilder.ToString());
            }
            // ... (نفس الـ Error Handling) ...
            catch (Exception ex) { Console.WriteLine($"AI API Error (Filter): {ex.Message}"); return null; }

        }

        public async Task<ContentRecommendationResponseDto?> GetContentBasedRecommendationsAsync(string bookTitle, int? topN = null)
        {
            if (string.IsNullOrWhiteSpace(bookTitle)) return null;

            var queryStringBuilder = new StringBuilder("recommendations/content");
            bool firstParam = true;
            Action<string, string?> appendQueryParam = (name, value) => {
                if (value != null)
                {
                    queryStringBuilder.Append(firstParam ? "?" : "&");
                    queryStringBuilder.Append($"{name}={Uri.EscapeDataString(value)}");
                    firstParam = false;
                }
            };

            appendQueryParam("title", bookTitle);
            appendQueryParam("top_n", topN?.ToString());

            try
            {
                return await _httpClient.GetFromJsonAsync<ContentRecommendationResponseDto>(queryStringBuilder.ToString());
            }
            // ... (نفس الـ Error Handling) ...
            catch (Exception ex) { Console.WriteLine($"AI API Error (Content): {ex.Message}"); return null; }
        }
    }
}