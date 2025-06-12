// ممكن تحط الكلاس ده فوق QuestionDto أو في ملف لوحده
using System.ComponentModel.DataAnnotations;
namespace Bookify.DTOs
{

    public class AnswerDto
    {
        [Required]
        public string Text { get; set; }
        public bool IsCorrect { get; set; }
    }
}