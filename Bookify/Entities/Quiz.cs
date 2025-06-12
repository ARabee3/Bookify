using System.ComponentModel.DataAnnotations;
namespace Bookify.Entities
{
    public class Quiz
    {
        [Key]
        public int QuizID { get; set; }
        public bool IsFinal { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public int? ChapterID { get; set; }
        public Chapter? Chapter { get; set; }
        public int? BookID { get; set; }
        public Book? Book { get; set; }
        public ICollection<Question> Questions { get; set; } = new List<Question>();
        public ICollection<UserQuizResult> QuizResults { get; set; } = new List<UserQuizResult>();
    
}
}
