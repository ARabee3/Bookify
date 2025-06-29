using System.Text.Json.Serialization;

namespace Bookify.DTOs.PdfProcessor
{
    public class PdfApiResponseDto<T>
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("data")]
        public T Data { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}