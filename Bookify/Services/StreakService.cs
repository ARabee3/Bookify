using Bookify.Contexts;
using Bookify.DTOs;
using Bookify.Entities;
using Bookify.Interfaces;
using Microsoft.AspNetCore.Identity; // عشان UserManager
using System;
using System.Threading.Tasks;

namespace Bookify.Services
{
    public class StreakService : IStreakService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context; // أو ممكن نعمل Unit of Work لو حابين

        public StreakService(UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task UpdateStreakAsync(string userId, DateTime activityDate)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                // المفروض المستخدم يكون موجود لو وصل لهنا
                Console.WriteLine($"StreakService: User not found with ID {userId}");
                return;
            }

            var today = activityDate.Date; // ناخد التاريخ فقط

            // لو مفيش نشاط سابق متسجل للـ Streak، أو لو تاريخ آخر نشاط للـ Streak مش النهارده
            if (user.LastStreakActivityDate == null || user.LastStreakActivityDate.Value.Date != today)
            {
                // نشوف هل نشاط النهارده بيكمل الـ Streak بتاع امبارح
                if (user.LastStreakActivityDate.HasValue && user.LastStreakActivityDate.Value.Date == today.AddDays(-1))
                {
                    // نعم، ده يوم متتالي
                    user.CurrentReadingStreak++;
                }
                else // لو لأ (يعني الـ Streak انكسر أو ده أول نشاط خالص)
                {
                    user.CurrentReadingStreak = 1; // نبدأ Streak جديد من 1
                }

                // نحدث أطول Streak لو الـ Current بقى أكبر
                if (user.CurrentReadingStreak > user.LongestReadingStreak)
                {
                    user.LongestReadingStreak = user.CurrentReadingStreak;
                }

                // نحدث تاريخ آخر نشاط للـ Streak بتاريخ النهارده
                user.LastStreakActivityDate = today;

                // نحفظ التغييرات في المستخدم
                // الـ UserManager.UpdateAsync هيحفظ التغييرات في جدول AspNetUsers
                // لو الـ Context مش بيعمل SaveChanges لوحده هنا، ممكن نحتاج نعملها
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    Console.WriteLine($"StreakService: Failed to update user streak for UserID {userId}. Errors: {string.Join(", ", updateResult.Errors.Select(e => e.Description))}");
                    // ممكن نعمل Log للأخطاء دي
                }
            }
            // لو تاريخ آخر نشاط للـ Streak هو النهارده، منعملش حاجة (لأن الـ Streak اتحسب مرة للنهارده)
        }

        public async Task<UserStreakDto?> GetUserStreakAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            // نتأكد لو الـ Streak الحالي المفروض يتصفر لو فات أكتر من يوم على آخر نشاط
            // دي خطوة مهمة عشان لو المستخدم مرجعش تاني يوم، الـ Current Streak يرجع صفر
            // إلا لو هو فتح التطبيق النهارده بس لسه معملش Log للـ Activity من الـ Progress
            if (user.LastStreakActivityDate.HasValue && user.LastStreakActivityDate.Value.Date < DateTime.UtcNow.Date.AddDays(-1))
            {
                // فات أكتر من يوم على آخر نشاط، يبقى الـ Current Streak اتكسر
                if (user.CurrentReadingStreak != 0) // نحدثه بس لو مكنش صفر أصلاً
                {
                    user.CurrentReadingStreak = 0;
                    await _userManager.UpdateAsync(user); // نحفظ التغيير
                }
            }


            return new UserStreakDto
            {
                CurrentStreak = user.CurrentReadingStreak,
                LongestStreak = user.LongestReadingStreak
            };
        }
    }
}