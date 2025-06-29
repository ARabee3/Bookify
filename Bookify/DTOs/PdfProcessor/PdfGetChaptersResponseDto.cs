using System.Text.Json.Serialization;

namespace Bookify.DTOs.PdfProcessor
{
    public class PdfGetChaptersResponseDto
    {
        [JsonPropertyName("chapters")]
        public List<PdfChapterDto> Chapters { get; set; }
    }
}