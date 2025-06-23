using Bookify.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookify.Interfaces
{
    public interface IProgressRepository
    {
        Task<Progress?> GetUserProgressForBookAsync(string userId, int bookId);
        Task<IEnumerable<Progress>> GetAllUserProgressAsync(string userId); // لجلب كل سجلات التقدم للمستخدم
        Task<IEnumerable<Progress>> GetCurrentlyReadingProgressAsync(string userId); // لجلب الكتب قيد القراءة
        Task AddProgressAsync(Progress progress);
        void Update(Progress progress); // EF Core يتتبع التغييرات
        // لا نحتاج Delete حالياً، التقدم لا يحذف عادةً
    }
}