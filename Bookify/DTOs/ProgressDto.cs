// في مجلد Dtos
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
        public string? BookPdfFileUrl { get; set; } // <<< تم إضافة هذه الخاصية
        public int? LastReadChapterID { get; set; }
        public string? LastReadChapterTitle { get; set; }
        public float CompletionPercentage { get; set; }
        public CompletionStatus Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? TotalPagesInBook { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }
}