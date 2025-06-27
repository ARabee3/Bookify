using Bookify.DTOs;
using Microsoft.AspNetCore.Http; // عشان IFormFile
using System.Threading.Tasks;
// ... (باقي الـ using)

namespace Bookify.Interfaces
{
    public interface IProfileService
    {
        Task<UserProfileDto?> GetUserProfileAsync(string userId);
        Task<bool> UpdateUserProfileAsync(string userId, UpdateProfileDto dto);
        Task<UserReadingStatsDto?> GetUserReadingStatsAsync(string userId);
        // --- الميثود الجديدة ---
        Task<string?> UploadProfilePictureAsync(string userId, IFormFile file); // هترجع الـ URL الكامل للصورة الجديدة أو null لو فشلت
        // --------------------
    }
}