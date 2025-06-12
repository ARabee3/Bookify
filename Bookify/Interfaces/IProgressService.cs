using Bookify.DTOs; // هنحتاج نعمل DTOs للـ Progress
using Bookify.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookify.Interfaces
{
    public interface IProgressService
    {
        // DTO للـ Input بتاع تحديث التقدم
        Task<ProgressDto?> UpdateOrCreateUserProgressAsync(string userId, UpdateProgressDto progressDto);
        Task<ProgressDto?> GetUserProgressForBookAsync(string userId, int bookId);
        Task<IEnumerable<ProgressDto>> GetAllUserProgressAsync(string userId);
        Task<IEnumerable<BookProgressDto>> GetCurrentlyReadingBooksAsync(string userId); // للـ UI بتاعة "Currently Reading"
    }
}