using Bookify.DTOs; // <<< تأكد من وجود UserStreakDto هنا
using Bookify.Entities; // <<< تأكد من وجود ApplicationUser هنا
using Bookify.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq; // <<< لإضافة Select في حالة الأخطاء
using System.Threading.Tasks;
// using Bookify.Contexts; // لم نعد بحاجة لـ AppDbContext هنا مباشرة إذا اعتمدنا على UserManager.UpdateAsync فقط

namespace Bookify.Services
{
    public class StreakService : IStreakService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        // لم نعد بحاجة لـ AppDbContext هنا إذا كانت UserManager.UpdateAsync كافية
        // private readonly AppDbContext _context;

        public StreakService(UserManager<ApplicationUser> userManager /*, AppDbContext context */)
        {
            _userManager = userManager;
            // _context = context;
        }

        public async Task UpdateStreakAsync(string userId, DateTime activityDate)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                Console.WriteLine($"StreakService: User not found with ID {userId} during streak update.");
                return; // لا يمكن تحديث الستريك لمستخدم غير موجود
            }

            var today = activityDate.Date; // نهتم بالتاريخ فقط

            // إذا لم يكن هناك تاريخ نشاط سابق للستريك، أو إذا كان تاريخ آخر نشاط للستريك ليس اليوم
            // فهذا يعني أننا بحاجة لتقييم الستريك لهذا اليوم.
            if (user.LastStreakActivityDate == null || user.LastStreakActivityDate.Value.Date != today)
            {
                bool streakLogicApplied = false;

                if (user.LastStreakActivityDate == null)
                {
                    // هذا أول نشاط يسجل للستريك على الإطلاق لهذا المستخدم
                    user.CurrentReadingStreak = 1;
                    streakLogicApplied = true;
                }
                else if (user.LastStreakActivityDate.Value.Date == today.AddDays(-1))
                {
                    // النشاط الحالي يكمل الستريك من اليوم السابق
                    user.CurrentReadingStreak++;
                    streakLogicApplied = true;
                }
                else if (user.LastStreakActivityDate.Value.Date < today.AddDays(-1))
                {
                    // مر يوم أو أكثر بدون نشاط، الستريك انكسر. نبدأ ستريك جديد بـ 1 لهذا اليوم.
                    user.CurrentReadingStreak = 1;
                    streakLogicApplied = true;
                }
                // الحالة الأخيرة: user.LastStreakActivityDate.Value.Date == today
                // وهذه الحالة تم التعامل معها بالشرط الخارجي (user.LastStreakActivityDate.Value.Date != today)
                // لذا لن ندخل هنا إذا كان آخر نشاط مسجل هو اليوم بالفعل.

                if (streakLogicApplied)
                {
                    // تحديث أطول ستريك إذا كان الستريك الحالي أكبر
                    if (user.CurrentReadingStreak > user.LongestReadingStreak)
                    {
                        user.LongestReadingStreak = user.CurrentReadingStreak;
                    }
                    // تحديث تاريخ آخر نشاط أثر في الستريك
                    user.LastStreakActivityDate = today;

                    var updateResult = await _userManager.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                    {
                        Console.WriteLine($"StreakService: Failed to update user streak for UserID {userId}. Errors: {string.Join(", ", updateResult.Errors.Select(e => e.Description))}");
                        // Consider logging these errors more formally
                    }
                }
            }
            // إذا كان LastStreakActivityDate هو نفسه تاريخ اليوم، فهذا يعني أن الستريك قد تم حسابه بالفعل لهذا اليوم
            // ولا يلزم إجراء أي تحديث إضافي للستريك نفسه.
        }

        public async Task<UserStreakDto?> GetUserStreakAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return null; // المستخدم غير موجود
            }

            // التحقق مما إذا كان يجب إعادة تعيين الستريك الحالي إلى صفر
            // إذا كان آخر نشاط مسجل للستريك أقدم من الأمس (أي مر يوم كامل بدون نشاط)
            if (user.LastStreakActivityDate.HasValue && user.LastStreakActivityDate.Value.Date < DateTime.UtcNow.Date.AddDays(-1))
            {
                if (user.CurrentReadingStreak != 0) // فقط إذا لم يكن بالفعل صفرًا
                {
                    user.CurrentReadingStreak = 0;
                    // لا حاجة لتحديث LastStreakActivityDate هنا، لأنه يعكس آخر يوم كان فيه نشاط فعلي
                    var resetResult = await _userManager.UpdateAsync(user);
                    if (!resetResult.Succeeded)
                    {
                        Console.WriteLine($"StreakService: Failed to reset user streak for UserID {userId}. Errors: {string.Join(", ", resetResult.Errors.Select(e => e.Description))}");
                    }
                }
            }

            return new UserStreakDto
            {
                CurrentStreak = user.CurrentReadingStreak,
                LongestStreak = user.LongestReadingStreak
                // يمكن إضافة LastStreakActivityDate هنا إذا كان الـ Frontend يحتاجها
                // LastActivityDate = user.LastStreakActivityDate
            };
        }
    }
}