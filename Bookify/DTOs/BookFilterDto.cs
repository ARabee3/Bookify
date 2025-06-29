namespace Bookify.DTOs
{
    // DTO لتجميع بارامترات الفلترة والـ Pagination
    public class BookFilterDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Category { get; set; }
        // يمكن إضافة بارامترات فلترة أخرى هنا لاحقاً
        // public string? Difficulty { get; set; }
        // public string? Author { get; set; }
    }
}