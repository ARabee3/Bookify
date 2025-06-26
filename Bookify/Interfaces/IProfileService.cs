using Bookify.DTOs;
using System.Threading.Tasks;

namespace Bookify.Interfaces
{
    public interface IProfileService
    {
        Task<UserProfileDto?> GetUserProfileAsync(string userId);
        Task<bool> UpdateUserProfileAsync(string userId, UpdateProfileDto dto);
        Task<UserReadingStatsDto?> GetUserReadingStatsAsync(string userId);
        // GetCurrentlyReadingBooksAsync موجودة في IProgressService
        // GetRecentlyCompletedBooksAsync ممكن نضيفها لـ IProgressService لو حبينا
    }
}