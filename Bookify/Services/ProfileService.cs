using Bookify.DTOs;
using Bookify.Entities;
using Bookify.Interfaces;
using Microsoft.AspNetCore.Hosting; // <<< مهمة عشان نوصل لمسار wwwroot
using Microsoft.AspNetCore.Http;    // <<< مهمة عشان IFormFile و IHttpContextAccessor
using Microsoft.AspNetCore.Identity;
using System;
using System.IO; // <<< مهمة عشان Path و FileStream
using System.Linq;
using System.Threading.Tasks;

namespace Bookify.Services
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProgressService _progressService;
        private readonly IStreakService _streakService;
        private readonly IWebHostEnvironment _webHostEnvironment; // <<< لإيجاد مسار wwwroot
        private readonly IHttpContextAccessor _httpContextAccessor; // <<< لبناء الـ Base URL

        public ProfileService(
            UserManager<ApplicationUser> userManager,
            IProgressService progressService,
            IStreakService streakService,
            IWebHostEnvironment webHostEnvironment, // <<< إضافة
            IHttpContextAccessor httpContextAccessor) // <<< إضافة
        {
            _userManager = userManager;
            _progressService = progressService;
            _streakService = streakService;
            _webHostEnvironment = webHostEnvironment; // <<< إضافة
            _httpContextAccessor = httpContextAccessor; // <<< إضافة
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var request = _httpContextAccessor.HttpContext?.Request;
            string? baseUrl = null;
            if (request != null)
            {
                baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
            }

            return new UserProfileDto
            {
                UserId = user.Id,
                Username = user.UserName ?? "N/A",
                Email = user.Email ?? "N/A",
                JoinedDate = null, // أو أي قيمة بديلة
                Age = user.Age,
                Specialization = user.Specialization,
                Level = user.Level,
                Interest = user.Interest,
                // --- بناء الـ URL الكامل لصورة البروفايل ---
                ProfilePictureFullUrl = !string.IsNullOrEmpty(user.ProfilePicturePath) && baseUrl != null
                                        ? $"{baseUrl}{user.ProfilePicturePath}" // افترضنا أن ProfilePicturePath يخزن المسار النسبي
                                        : null // أو لينك لصورة افتراضية
                // -----------------------------------------
            };
        }

        public async Task<bool> UpdateUserProfileAsync(string userId, UpdateProfileDto dto)
        {
            // ... (الكود بتاع تعديل البروفايل زي ما هو) ...
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            bool needsUpdate = false;
            if (!string.IsNullOrWhiteSpace(dto.Username) && user.UserName != dto.Username)
            {
                var existingUserWithNewUsername = await _userManager.FindByNameAsync(dto.Username);
                if (existingUserWithNewUsername != null && existingUserWithNewUsername.Id != userId) return false; // Username taken
                var setUsernameResult = await _userManager.SetUserNameAsync(user, dto.Username);
                if (!setUsernameResult.Succeeded) return false;
                needsUpdate = true;
            }
            if (dto.Age.HasValue && user.Age != dto.Age.Value) { user.Age = dto.Age.Value; needsUpdate = true; }
            if (dto.Specialization != null && user.Specialization != dto.Specialization) { user.Specialization = dto.Specialization; needsUpdate = true; }
            if (dto.Level != null && user.Level != dto.Level) { user.Level = dto.Level; needsUpdate = true; }
            if (dto.Interest != null && user.Interest != dto.Interest) { user.Interest = dto.Interest; needsUpdate = true; }

            if (needsUpdate) return (await _userManager.UpdateAsync(user)).Succeeded;
            return true;
        }

        public async Task<UserReadingStatsDto?> GetUserReadingStatsAsync(string userId)
        {
            // ... (الكود بتاع إحصائيات القراءة زي ما هو) ...
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


        // --- بداية ميثود رفع صورة البروفايل ---
        public async Task<string?> UploadProfilePictureAsync(string userId, IFormFile file)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                // throw new KeyNotFoundException("User not found.");
                return null;
            }

            if (file == null || file.Length == 0)
            {
                // throw new ArgumentException("No file uploaded or file is empty.");
                return null;
            }

            // 1. التحقق من نوع الملف (مثلاً jpg, png) وحجمه (اختياري)
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                // throw new ArgumentException("Invalid file type. Only JPG, PNG, GIF are allowed.");
                return null;
            }
            // (اختياري) التحقق من حجم الملف
            // if (file.Length > 5 * 1024 * 1024) // 5MB limit
            // {
            //     throw new ArgumentException("File size exceeds the limit of 5MB.");
            // }

            // 2. تحديد مسار حفظ الملف واسم فريد
            // اسم المجلد جوه wwwroot
            string profilePicturesFolder = "ProfilePictures";
            // المسار الكامل لمجلد wwwroot
            string wwwRootPath = _webHostEnvironment.WebRootPath; // بيجيب المسار الفعلي لـ wwwroot على السيرفر
            if (string.IsNullOrEmpty(wwwRootPath))
            {
                // حالة نادرة، لو wwwroot مش متظبط صح
                Console.WriteLine("wwwRootPath is null or empty. Cannot save file.");
                return null;
            }
            // المسار الكامل للمجلد اللي هنحفظ فيه الصور
            string targetFolderPath = Path.Combine(wwwRootPath, profilePicturesFolder);
            if (!Directory.Exists(targetFolderPath)) // لو المجلد مش موجود، نعمله
            {
                Directory.CreateDirectory(targetFolderPath);
            }

            // إنشاء اسم ملف فريد (ممكن نستخدم الـ UserID عشان نضمن إن كل مستخدم ليه صورة واحدة أو نستخدم GUID)
            // استخدام الـ UserID مع امتداد الملف الأصلي
            string uniqueFileName = $"{userId}{fileExtension}";
            string fullFilePathOnServer = Path.Combine(targetFolderPath, uniqueFileName);

            // 3. حذف الصورة القديمة (لو موجودة) - (اختياري لكن موصى به)
            if (!string.IsNullOrEmpty(user.ProfilePicturePath))
            {
                string oldFilePathOnServer = Path.Combine(wwwRootPath, user.ProfilePicturePath.TrimStart('/')); // نشيل الـ / من الأول
                if (File.Exists(oldFilePathOnServer) && oldFilePathOnServer != fullFilePathOnServer) // نتأكد إنه مش نفس الملف
                {
                    try
                    {
                        File.Delete(oldFilePathOnServer);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error deleting old profile picture {oldFilePathOnServer}: {ex.Message}");
                        // نكمل عادي حتى لو معرفناش نحذف القديمة
                    }
                }
            }

            // 4. حفظ الملف الجديد على السيرفر
            try
            {
                using (var stream = new FileStream(fullFilePathOnServer, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving profile picture {fullFilePathOnServer}: {ex.Message}");
                return null; // فشل الحفظ
            }


            // 5. تحديث المسار النسبي للصورة في قاعدة البيانات للمستخدم
            string relativePath = $"/{profilePicturesFolder}/{uniqueFileName}"; // المسار اللي هيتخزن في الداتا بيز
            user.ProfilePicturePath = relativePath;
            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                // لو فشل تحديث الداتا بيز، ممكن نحاول نحذف الملف اللي لسه رافعينه (Rollback بسيط)
                if (File.Exists(fullFilePathOnServer)) File.Delete(fullFilePathOnServer);
                Console.WriteLine($"Failed to update user profile picture path in DB for user {userId}.");
                return null;
            }

            // 6. بناء الـ URL الكامل للصورة عشان نرجعه
            var request = _httpContextAccessor.HttpContext?.Request;
            string? baseUrl = null;
            if (request != null)
            {
                baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
            }

            return !string.IsNullOrEmpty(relativePath) && baseUrl != null ? $"{baseUrl}{relativePath}" : null;
        }
        // --- نهاية ميثود رفع صورة البروفايل ---
    }
}