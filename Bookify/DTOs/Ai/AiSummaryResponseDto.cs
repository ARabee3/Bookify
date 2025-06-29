using System.Text.Json.Serialization;
namespace Bookify.DTOs.Ai
{
    public class AiSummaryDataDto
    {
        [JsonPropertyName("chapter_title")]
        public string ChapterTitle { get; set; }
        [JsonPropertyName("summary")]
        public string SummaryText { get; set; }
        [JsonPropertyName("start_page")]
        public int StartPage { get; set; }
        [JsonPropertyName("end_page")]
        public int EndPage { get; set; }
    }
    public class AiSummaryResponseDto
    {
        [JsonPropertyName("data")]
        public AiSummaryDataDto Data { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}