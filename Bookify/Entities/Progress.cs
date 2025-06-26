using System;
using System.ComponentModel.DataAnnotations;

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
        public string UserID { get; set; }
        public virtual ApplicationUser User { get; set; }

        [Required]
        public int BookID { get; set; }
        public virtual Book Book { get; set; }

        public float CompletionPercentage { get; set; } // From 0 to 100
        public CompletionStatus Status { get; set; } = CompletionStatus.NotStarted;

        // public int? LastReadChapterID { get; set; } // <<< تم الحذف
        // public virtual Chapter? LastReadChapter { get; set; } // <<< تم الحذف

        public int? LastReadPageNumber { get; set; } // <<< أبقينا على هذا (أو يمكن إضافته لو لم يكن موجوداً)

        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}