using Bookify.Entities;
using System;

namespace Bookify.DTOs
{
    public class ProgressDto
    {
        public int ProgressID { get; set; }
        public int BookID { get; set; }
        public string? BookTitle { get; set; }
        public string? BookCoverImageUrl { get; set; }
        public string? BookPdfFileUrl { get; set; }
        // public int? LastReadChapterID { get; set; } // <<< تم الحذف
        // public string? LastReadChapterTitle { get; set; } // <<< تم الحذف
        public int? LastReadPageNumber { get; set; } // <<< تمت الإضافة (إذا كانت موجودة في Progress Entity)
        public float CompletionPercentage { get; set; }
        public CompletionStatus Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? TotalPagesInBook { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }
}