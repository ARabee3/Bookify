using Bookify.DTOs;         // عشان UserStreakDto
using Bookify.Interfaces;    // عشان IStreakService
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;               // عشان Exception
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bookify.Controllers
{
    [Route("api/[controller]")] // المسار: /api/streak (لو اسم الكلاس StreakController)
    [ApiController]
    [Authorize]
    public class StreakController : ControllerBase // تم تغيير الاسم من StreaksController
    {
        private readonly IStreakService _streakService;

        public StreakController(IStreakService streakService)
        {
            _streakService = streakService;
        }

        // GET /api/streak/my
        [HttpGet("my")]
        public async Task<IActionResult> GetMyStreakAsync() // تم تعديل اسم الميثود
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized(new { Message = "User ID could not be determined from token." });
            }

            try
            {
                var streakDto = await _streakService.GetUserStreakAsync(userId);

                // الـ Service الآن ترجع DTO بقيم صفرية إذا كان المستخدم جديداً أو الستريك مكسور
                // ولا ترجع null إلا إذا كان المستخدم نفسه غير موجود (وهو ما لا يجب أن يحدث هنا بسبب Authorize)
                if (streakDto == null)
                {
                    // هذا يعتبر خطأ غير متوقع في الـ Service Logic
                    Console.WriteLine($"StreakService.GetUserStreakAsync returned null for user {userId} which should not happen.");
                    return StatusCode(StatusCodes.Status500InternalServerError, "Could not retrieve streak data.");
                }

                return Ok(streakDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetMyStreakAsync for UserID {userId}: {ex.ToString()}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while retrieving your reading streak.");
            }
        }
    }
}