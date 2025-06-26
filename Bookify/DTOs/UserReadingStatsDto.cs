namespace Bookify.DTOs
{
    public class UserReadingStatsDto
    {
        public int BooksReadCount { get; set; }
        public int LongestStreak { get; set; }
        public int CurrentStreak { get; set; } // نضيف الـ Current Streak كمان
    }
}