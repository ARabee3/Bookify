using System.ComponentModel;

namespace Bookify.DTOs.Ai
{
    public class FilterCriteriaDto // ده هنستخدمه كباراميتر للفلترة في الـ Controller
    {
        public string? Category { get; set; }
        public string? Difficulty { get; set; }
        public string? Language { get; set; }
        public int? MinViews { get; set; }
        public float? MinRating { get; set; }
        public string? Author { get; set; }
        public int? RecentYears { get; set; }

        [DefaultValue(1)]
        public int PageNumber { get; set; } = 1;

        [DefaultValue(10)]
        public int PageSize { get; set; } = 10;
    }
}