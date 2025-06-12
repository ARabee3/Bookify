using System.ComponentModel.DataAnnotations;
namespace Bookify.Entities
{
    public class Question
    {
        [Key]
        public int QuestionID { get; set; }
        public string Text { get; set; }

        public int QuizID { get; set; }
        public Quiz Quiz { get; set; }

        public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>(); // <<< ضيف ده
    }
}
