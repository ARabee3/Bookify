using System.Collections.Generic;
using System.Text.Json.Serialization;
namespace Bookify.DTOs.Ai
{
    public class AiQuestionDto
    {
        [JsonPropertyName("question")]
        public string QuestionText { get; set; }
        [JsonPropertyName("choices")]
        public List<string> Choices { get; set; }
        [JsonPropertyName("answer")]
        public string CorrectAnswer { get; set; }
    }
    public class AiQuizDataDto
    {
        [JsonPropertyName("quiz_title")]
        public string QuizTitle { get; set; }
        [JsonPropertyName("questions")]
        public List<AiQuestionDto> Questions { get; set; }
        [JsonPropertyName("start_page")]
        public int StartPage { get; set; }
        [JsonPropertyName("end_page")]
        public int EndPage { get; set; }
    }
    public class AiQuizResponseDto
    {
        [JsonPropertyName("data")]
        public AiQuizDataDto Data { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}