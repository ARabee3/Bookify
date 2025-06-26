using Bookify.DTOs;
using Bookify.Entities;
using Bookify.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Bookify.Services
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProgressService _progressService;
        private readonly IStreakService _streakService;

        public ProfileService(
            UserManager<ApplicationUser> userManager,
            IProgressService progressService,
            IStreakService streakService)
        {
            _userManager = userManager;
            _progressService = progressService;
            _streakService = streakService;
        }

        // ... (GetUserProfileAsync و GetUserReadingStatsAsync كما هما) ...
        public async Task<UserProfileDto?> GetUserProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            DateTime? joinedDate = user.LockoutEnd?.UtcDateTime; // كمؤشر مؤقت إذا لم يكن لديك تاريخ تسجيل مخصص
            // من الأفضل إضافة CreatedAtUtc لـ ApplicationUser وتعبئتها عند التسجيل

            return new UserProfileDto
            {
                UserId = user.Id,
                Username = user.UserName ?? "N/A",
                Email = user.Email ?? "N/A",
                JoinedDate = joinedDate,
                Age = user.Age,
                Specialization = user.Specialization,
                Level = user.Level,
                Interest = user.Interest
            };
        }


        public async Task<UserReadingStatsDto?> GetUserReadingStatsAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var streakData = await _streakService.GetUserStreakAsync(userId);
            var allProgress = await _progressService.GetAllUserProgressAsync(userId);
            int booksReadCount = allProgress.Count(p => p.Status == CompletionStatus.Completed);

            return new UserReadingStatsDto
            {
                BooksReadCount = booksReadCount,
                LongestStreak = user.LongestReadingStreak,
                CurrentStreak = streakData?.CurrentStreak ?? 0
            };
        }


        // --- تم تعديل هذه الميثود ---
        public async Task<bool> UpdateUserProfileAsync(string userId, UpdateProfileDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false; // المستخدم غير موجود
            }

            bool needsUpdate = false;

            // تحديث الـ Username (إذا تم إرساله وتغير)
            if (!string.IsNullOrWhiteSpace(dto.Username) && user.UserName != dto.Username)
            {
                // تحقق إذا كان الـ Username الجديد مستخدم من قبل (اختياري لكن مهم)
                var existingUserWithNewUsername = await _userManager.FindByNameAsync(dto.Username);
                if (existingUserWithNewUsername != null && existingUserWithNewUsername.Id != userId)
                {
                    // throw new ArgumentException("Username is already taken."); // أو نرجع false مع رسالة خطأ
                    return false; // مؤقتاً نرجع false، الـ Controller ممكن يرجع 409 Conflict
                }
                // Identity يتطلب تعديل UserName و NormalizedUserName
                var setUsernameResult = await _userManager.SetUserNameAsync(user, dto.Username);
                if (!setUsernameResult.Succeeded) return false; // فشل تحديث الـ Username
                needsUpdate = true;
            }

            // تحديث باقي الخصائص
            if (dto.Age.HasValue && user.Age != dto.Age.Value)
            {
                user.Age = dto.Age.Value;
                needsUpdate = true;
            }
            if (dto.Specialization != null && user.Specialization != dto.Specialization) // استخدام dto.Specialization مباشرة
            {
                user.Specialization = dto.Specialization;
                needsUpdate = true;
            }
            if (dto.Level != null && user.Level != dto.Level) // استخدام dto.Level مباشرة
            {
                user.Level = dto.Level;
                needsUpdate = true;
            }
            if (dto.Interest != null && user.Interest != dto.Interest) // استخدام dto.Interest مباشرة
            {
                user.Interest = dto.Interest;
                needsUpdate = true;
            }

            if (needsUpdate) // فقط إذا كان هناك تغيير فعلي
            {
                var updateResult = await _userManager.UpdateAsync(user); // هذا سيحفظ كل التغييرات (بما فيها الـ Username)
                return updateResult.Succeeded;
            }

            return true; // لم يتم إجراء أي تغيير، نعتبره نجاحًا (أو ممكن نرجع false لو عايزين نقول مفيش حاجة اتغيرت)
        }
    }
}