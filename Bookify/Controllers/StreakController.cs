using Bookify.DTOs; // عشان UserStreakDto
using Bookify.Interfaces; // عشان IStreakService
using Microsoft.AspNetCore.Authorization; // عشان [Authorize]
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims; // عشان ClaimTypes
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http; // عشان StatusCodes

namespace Bookify.Controllers
{
    [Route("api/[controller]")] // المسار هيبقى /api/streaks
    [ApiController]
    [Authorize] // <<< كل الـ Endpoints هنا هتتطلب تسجيل دخول
    public class StreakController : ControllerBase
    {
        private readonly IStreakService _streakService;

        public StreakController(IStreakService streakService)
        {
            _streakService = streakService;
        }

        // --- Endpoint لجلب بيانات الـ Streak للمستخدم الحالي ---
        // GET /api/streaks/my
        [HttpGet("my")]
        public async Task<IActionResult> GetMyStreakAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                // هذا لا يجب أن يحدث بسبب [Authorize]
                return Unauthorized("User ID could not be determined from token.");
            }

            try
            {
                var streakDto = await _streakService.GetUserStreakAsync(userId);

                if (streakDto == null)
                {
                    // حالة أن المستخدم لم يتم العثور عليه (نادرة جداً لو الـ token سليم)
                    // أو ممكن الـ Service ترجع DTO بقيم صفرية لو المستخدم جديد ولسه ملوش streak
                    // حالياً، GetUserStreakAsync في Service بترجع DTO بقيم صفرية لو المستخدم جديد
                    // لكن لو رجعت null، ممكن نرجع NotFound أو Ok مع قيم صفرية.
                    // دعنا نفترض أن الـ Service دائماً سترجع DTO طالما المستخدم موجود.
                    return Ok(new UserStreakDto { CurrentStreak = 0, LongestStreak = 0 }); // قيمة افتراضية
                }

                return Ok(streakDto);
            }
            catch (Exception ex)
            {
                // يفضل استخدام Logger حقيقي
                Console.WriteLine($"Error getting user streak for UserID {userId}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving your reading streak.");
            }
        }
    }
}