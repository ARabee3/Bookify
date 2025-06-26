using System.Collections.Generic;

namespace Bookify.DTOs
{
    public class ProfilePageDto
    {
        public UserProfileDto UserProfile { get; set; }
        public UserReadingStatsDto ReadingStats { get; set; }
        public IEnumerable<BookProgressDto> CurrentlyReadingBooks { get; set; } // <<< تم التعليق أو الحذف
        public IEnumerable<BookListItemDto> RecentlyCompletedBooks { get; set; } // <<< تم إضافة هذه
    }
}