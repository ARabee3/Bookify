using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookify.Entities
{
    public enum CompletionStatus
    {
        NotStarted = 0,
        InProgress = 1,
        Completed = 2
    }

    public class Progress
    {
        [Key]
        public int ProgressID { get; set; }

        [Required]
        public string UserID { get; set; } // Foreign Key to ApplicationUser
        public virtual ApplicationUser User { get; set; }

        [Required]
        public int BookID { get; set; }   // Foreign Key to Book
        public virtual Book Book { get; set; }

        public float CompletionPercentage { get; set; } // From 0 to 100

        public CompletionStatus Status { get; set; } = CompletionStatus.NotStarted;

        public int? LastReadChapterID { get; set; } // Nullable if progress is by page or general
        public virtual Chapter? LastReadChapter { get; set; }

        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow; // To track the last update
        public DateTime? StartDate { get; set; } // When the user started the book
        public DateTime? EndDate { get; set; }   // When the user completed the book
    }
}