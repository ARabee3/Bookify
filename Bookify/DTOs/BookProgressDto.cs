using Microsoft.AspNetCore.Http; // عشان IHttpContextAccessor لو هنبني الـ URL هنا
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
        public int? LastReadChapterID { get; set; }
        // public string? LastReadChapterTitle { get; set; } // (اختياري)
        public int? TotalPages { get; set; }
        public DateTime LastUpdatedAt { get; set; } // لعرض آخر نشاط على الكتاب
    }
}