namespace Bookify.DTOs
{
    public class BookListItemDto
    {
        public int BookID { get; set; }
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? Category { get; set; }
        public string? CoverImageUrl { get; set; }
        public float? AverageRating { get; set; }

        // --- الخصائص الجديدة المضافة ---
        public string? Difficulty { get; set; }
        public int? Views { get; set; }
        public string? Language { get; set; }
        public int? ReleaseYear { get; set; }
        public string? Prerequisites { get; set; }
        public string? LearningObjectives { get; set; }
        // public string? Summary { get; set; } // <<< هل نضيف الـ Summary هنا كمان؟ عادة بيكون طويل للقائمة
        public int? TotalPages { get; set; } // <<< ممكن نضيف عدد الصفحات الكلي
        // -------------------------------
    }
}