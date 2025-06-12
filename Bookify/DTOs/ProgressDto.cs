using Bookify.Entities; // عشان CompletionStatus

namespace Bookify.DTOs
{
    public class ProgressDto
    {
        public int ProgressID { get; set; }
        public int BookID { get; set; }
        // ممكن نرجع اسم الكتاب هنا لو هنعمل Join أو Mapping
        public string? BookTitle { get; set; }
        public int? LastReadChapterID { get; set; }
        public string? LastReadChapterTitle { get; set; } // ممكن نضيفه
        public float CompletionPercentage { get; set; }
        public CompletionStatus Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}