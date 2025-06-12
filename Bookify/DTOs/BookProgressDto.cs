namespace Bookify.DTOs
{
    public class BookProgressDto // شبيه بـ BookDto بس مع معلومات التقدم
    {
        public int BookID { get; set; }
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? Category { get; set; }
        public string? PdfFilePath { get; set; } // عشان لينك الـ PDF
        public string? CoverImageUrl { get; set; } // لو عندك صور أغلفة
        public float CompletionPercentage { get; set; } // من جدول Progress
        public int? LastReadChapterID { get; set; } // من جدول Progress
        // ممكن تضيف TotalPages من Book لو هتعرض "X / Y pages"
        public int? TotalPages { get; set; }
    }
}