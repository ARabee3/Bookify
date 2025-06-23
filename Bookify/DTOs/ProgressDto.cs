using Bookify.Entities; // عشان CompletionStatus
using System;
namespace Bookify.DTOs
{
        public class ProgressDto
        {
            public int ProgressID { get; set; }
            public int BookID { get; set; }
            public string? BookTitle { get; set; }
            public string? BookCoverImageUrl { get; set; } // لعرض صورة الكتاب
            public int? LastReadChapterID { get; set; }
            public string? LastReadChapterTitle { get; set; }
            public float CompletionPercentage { get; set; }
            public CompletionStatus Status { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public int? TotalPagesInBook { get; set; } // من Book.TotalPages
            public DateTime LastUpdatedAt { get; set; }
        }
    }
