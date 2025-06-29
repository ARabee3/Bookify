using System.Text.Json.Serialization;

namespace Bookify.DTOs.PdfProcessor
{
    public class PdfChapterDto
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("start_page")]
        public int StartPage { get; set; }

        [JsonPropertyName("end_page")]
        public int EndPage { get; set; }
    }
}