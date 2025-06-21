namespace Bookify.DTOs.Ai
{
    public class FilterCriteriaDto // ده هنستخدمه كباراميتر للفلترة في الـ Controller
    {
        public string? Genre { get; set; }
        public string? Difficulty { get; set; }
        public string? Language { get; set; }
        public int? MinViews { get; set; }
        public float? MinRating { get; set; }
        public string? Author { get; set; }
        public int? RecentYears { get; set; }
        public int? TopN { get; set; } // الـ AI API ممكن متكونش بتستخدمه للفلتر، بس ممكن نضيفه لو هنفلتر بعدين
    }
}