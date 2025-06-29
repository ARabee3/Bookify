using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Bookify.DTOs.Ai
{
    // يمثل الـ object الداخلي { "end_page": 8, "start_page": 2, "title": "..." }
    public class AiChapterDto
    {
        [JsonPropertyName("end_page")]
        public int EndPage { get; set; }

        [JsonPropertyName("start_page")]
        public int StartPage { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }
    }

    // يمثل الـ object الداخلي { "chapters": [...] }
    public class AiChapterDataDto
    {
        [JsonPropertyName("chapters")]
        public List<AiChapterDto> Chapters { get; set; } = new List<AiChapterDto>();
    }

    // يمثل الـ JSON Response الكامل
    public class AiGetChaptersResponseDto
    {
        [JsonPropertyName("data")]
        public AiChapterDataDto Data { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}