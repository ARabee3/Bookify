using System;
using System.ComponentModel.DataAnnotations;

namespace Bookify.Entities
{
    public enum JobStatus
    {
        Pending,
        Processing,
        Completed,
        Failed
    }

    public class BookProcessingJob
    {
        [Key]
        public Guid JobId { get; set; } = Guid.NewGuid();

        [Required]
        public string OriginalFileName { get; set; }

        [Required]
        public string StoredFilePath { get; set; } // Relative path to the PDF in wwwroot

        [Required]
        public string UserId { get; set; }

        public JobStatus Status { get; set; } = JobStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public string? ErrorMessage { get; set; }

        // When the job is done, we link it to the final book entity
        public int? ResultingBookId { get; set; }
        public virtual Book? ResultingBook { get; set; }
    }
}