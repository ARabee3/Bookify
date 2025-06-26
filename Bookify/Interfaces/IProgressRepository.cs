using Bookify.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookify.Interfaces
{
    public interface IProgressRepository
    {
        Task<Progress?> GetUserProgressForBookAsync(string userId, int bookId);
        Task<IEnumerable<Progress>> GetAllUserProgressAsync(string userId);
        Task<IEnumerable<Progress>> GetCurrentlyReadingProgressAsync(string userId);
        Task AddAsync(Progress progress); // تم تغيير الاسم ليكون أوضح
        void Update(Progress progress); // تم تغيير الاسم ليكون أوضح
    }
}