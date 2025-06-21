namespace Bookify.DTOs // أو الـ Namespace المناسب لمشروعك
{
    public class ChapterSummaryDto
    {
        public int ChapterID { get; set; }
        public string? Title { get; set; }
        public int ChapterNumber { get; set; }
        // ممكن نضيف أي خاصية تانية بسيطة من الشابتر لو محتاجينها هنا
    }
}