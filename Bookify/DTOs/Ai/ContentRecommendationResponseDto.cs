using System.Collections.Generic;
using System.Text.Json.Serialization;
namespace Bookify.DTOs.Ai
{
    public class ContentRecommendationResponseDto
    {
        [JsonPropertyName("input_title")]
        public string? InputTitle { get; set; }
        [JsonPropertyName("recommendations")]
        public List<string>? Recommendations { get; set; }
    }
}