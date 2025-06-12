using Bookify.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookify.Interfaces
{
    public interface IProgressRepository
    {
        Task<Progress?> GetUserProgressForBookAsync(string userId, int bookId);
        Task<IEnumerable<Progress>> GetAllUserProgressAsync(string userId);
        Task AddProgressAsync(Progress progress);
        Task UpdateProgressAsync(Progress progress); // ممكن ندمج Add و Update في ميثود واحدة (Upsert)
        // Task<bool> HasActivityTodayAsync(string userId, DateTime today); // ميثود مساعدة للـ Activity Log
        // Task AddDailyActivityLogAsync(UserDailyActivityLog activityLog); // ميثود مساعدة للـ Activity Log
    }
}