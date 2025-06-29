using System.Text.Json.Serialization;

namespace Bookify.DTOs.PdfProcessor
{
    // DTO for the Quiz data returned from the AI API
    public class PdfQuizResponseDto
    {
        [JsonPropertyName("quiz_title")]
        public string QuizTitle { get; set; }

        [JsonPropertyName("questions")]
        public List<PdfQuizQuestionDto> Questions { get; set; }
    }

    // DTO for a single question from the AI API
    public class PdfQuizQuestionDto
    {
        [JsonPropertyName("question")]
        public string Question { get; set; }

        [JsonPropertyName("choices")]
        public List<string> Choices { get; set; }

        [JsonPropertyName("answer")]
        public string Answer { get; set; }
    }

    // DTO we will return to our frontend (cleaner)
    public class ChapterQuizDto
    {
        public string QuizTitle { get; set; }
        public List<PdfQuizQuestionDto> Questions { get; set; }
    }
}