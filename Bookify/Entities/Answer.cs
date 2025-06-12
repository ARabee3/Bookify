using System.ComponentModel.DataAnnotations;

namespace Bookify.Entities
{
    public class Answer
    {
        [Key]
        public int AnswerID { get; set; } // Primary Key for Answer

        [Required] // Text of the answer option cannot be null
        public string Text { get; set; }

        public bool IsCorrect { get; set; } // Is this the correct answer?

        // Foreign Key to link back to the Question
        public int QuestionID { get; set; }
        public virtual Question Question { get; set; } // Navigation property
    }
}
