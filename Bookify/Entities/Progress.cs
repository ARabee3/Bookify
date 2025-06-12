using System.ComponentModel.DataAnnotations;
namespace Bookify.Entities
{
    public enum CompletionStatus { NotStarted, InProgress, Completed }
    public class Progress
    {
        [Key]
        public int ProgressID { get; set; }
        public string UserID { get; set; }
        public int BookID { get; set; }
        public float CompletionPercentage { get; set; }
        public CompletionStatus Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public ApplicationUser User { get; set; }
        public Book Book { get; set; }

        // في Progress.cs
        public int? LastReadChapterID { get; set; } // ID بتاع آخر شابتر اليوزر وقف عنده (Nullable)
        public virtual Chapter? LastReadChapter { get; set; } // Navigation property (اختياري)
    }
}
