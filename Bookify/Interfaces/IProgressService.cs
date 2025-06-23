using Bookify.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookify.Interfaces
{
    public interface IProgressService
    {
        Task<ProgressDto?> UpdateOrCreateUserProgressAsync(string userId, UpdateProgressDto progressDto);
        Task<ProgressDto?> GetUserProgressForBookAsync(string userId, int bookId);
        Task<IEnumerable<ProgressDto>> GetAllUserProgressAsync(string userId);
        Task<IEnumerable<BookProgressDto>> GetCurrentlyReadingBooksAsync(string userId);
    }
}