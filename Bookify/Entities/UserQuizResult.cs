using System.ComponentModel.DataAnnotations;
namespace Bookify.Entities
{
    public class UserQuizResult
    {
        [Key]
        public int ResultID { get; set; }
        public DateTime AttemptDate { get; set; } = DateTime.Now;
        public float Score { get; set; }

        public string UserID { get; set; }
        public ApplicationUser User { get; set; }

        public int QuizID { get; set; }
        public Quiz Quiz { get; set; }
    }
}
