using Bookify.DTOs; // هنعمل DTO لعرض الـ Streak
using System.Threading.Tasks;

namespace Bookify.Interfaces
{
    public interface IStreakService
    {
        // ميثود لتحديث الـ Streak للمستخدم بعد تسجيل نشاط
        // هتاخد UserID وتاريخ النشاط الحالي
        Task UpdateStreakAsync(string userId, DateTime activityDate);

        // ميثود لجلب بيانات الـ Streak للمستخدم الحالي
        Task<UserStreakDto?> GetUserStreakAsync(string userId);
    }
}