using System; // لإضافة DateTime

namespace Bookify.DTOs
{
    public class BookProgressDto
    {
        public int BookID { get; set; }
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? Category { get; set; }
        public string? CoverImageUrl { get; set; }
        public float CompletionPercentage { get; set; }
        // public int? LastReadChapterID { get; set; } // <<< تم الحذف
        public int? LastReadPageNumber { get; set; } // <<< تمت الإضافة (إذا كانت موجودة في Progress Entity)
        public int? TotalPages { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }
}