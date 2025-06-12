using System.ComponentModel.DataAnnotations;
namespace Bookify.DTOs
{
    public class QuestionDto
    {
        [Required]
        public string Text { get; set; }
        public List<AnswerDto> Answers { get; set; } // قائمة بالإجابات المحتملة
    }
}