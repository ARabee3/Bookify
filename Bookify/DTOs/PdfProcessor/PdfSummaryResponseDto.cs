using System.Text.Json.Serialization;

namespace Bookify.DTOs.PdfProcessor
{
    public class PdfSummaryResponseDto
    {
        [JsonPropertyName("chapter_title")]
        public string ChapterTitle { get; set; }

        [JsonPropertyName("summary")]
        public string Summary { get; set; }
    }
}