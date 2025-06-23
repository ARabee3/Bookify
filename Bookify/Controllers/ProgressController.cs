using Bookify.DTOs;
using Bookify.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic; // لإضافة List
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bookify.Controllers
{
    [Route("api/[controller]")] // المسار الأساسي: /api/progress
    [ApiController]
    [Authorize] // كل الـ Endpoints هنا مؤمنة
    public class ProgressController : ControllerBase
    {
        private readonly IProgressService _progressService;

        public ProgressController(IProgressService progressService)
        {
            _progressService = progressService;
        }

        // POST /api/progress  (لتحديث أو إنشاء سجل التقدم)
        [HttpPost]
        public async Task<IActionResult> UpdateOrCreateProgress([FromBody] UpdateProgressDto progressDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                // هذا لا يجب أن يحدث بسبب [Authorize]
                return Unauthorized(new { Message = "User ID could not be determined from token." });
            }

            try
            {
                var resultDto = await _progressService.UpdateOrCreateUserProgressAsync(userId, progressDto);

                // الـ Service الآن ترمي Exception في حالة عدم وجود الكتاب أو الشابتر
                // لذا لا نحتاج للتحقق من null هنا بنفس الطريقة السابقة إذا اعتمدنا على الـ Exception Handling

                return Ok(resultDto); // نرجع الـ ProgressDto المحدث
            }
            catch (KeyNotFoundException knfEx) // لو الكتاب غير موجود
            {
                return NotFound(new { Message = knfEx.Message });
            }
            catch (ArgumentException argEx) // لو الشابتر غير صالح أو لا يتبع الكتاب
            {
                return BadRequest(new { Message = argEx.Message });
            }
            catch (Exception ex) // للأخطاء العامة غير المتوقعة
            {
                // يفضل استخدام Logger حقيقي هنا
                Console.WriteLine($"Error in UpdateOrCreateProgress for user {userId}: {ex.ToString()}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred while updating progress." });
            }
        }

        // GET /api/progress/book/{bookId}  (لجلب التقدم لكتاب معين)
        [HttpGet("book/{bookId}")]
        public async Task<IActionResult> GetBookProgress(int bookId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized("User not identified.");

            try
            {
                var progressDto = await _progressService.GetUserProgressForBookAsync(userId, bookId);
                if (progressDto == null)
                {
                    // إذا لم يوجد سجل تقدم، نرجع DTO افتراضي يوضح أن التقدم لم يبدأ
                    // أو ممكن نرجع 204 No Content أو 404 Not Found حسب ما يفضل الـ Frontend
                    return Ok(new ProgressDto
                    {
                        BookID = bookId,
                        CompletionPercentage = 0,
                        Status = Entities.CompletionStatus.NotStarted
                        // يمكن ملء باقي الحقول بقيم افتراضية لو الـ Frontend يحتاجها
                    });
                }
                return Ok(progressDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting progress for book {bookId} by user {userId}: {ex.ToString()}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        // GET /api/progress/my/all (لجلب كل سجلات التقدم للمستخدم)
        [HttpGet("my/all")]
        public async Task<IActionResult> GetAllMyProgress()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized("User not identified.");

            try
            {
                var progressesDto = await _progressService.GetAllUserProgressAsync(userId);
                return Ok(progressesDto); // سترجع قائمة فارغة إذا لم يوجد تقدم
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all progress for user {userId}: {ex.ToString()}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        // GET /api/progress/my/currently-reading (لجلب الكتب التي يقرأها المستخدم حالياً)
        [HttpGet("my/currently-reading")]
        public async Task<IActionResult> GetMyCurrentlyReadingBooks()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized("User not identified.");

            try
            {
                var booksInProgressDto = await _progressService.GetCurrentlyReadingBooksAsync(userId);
                return Ok(booksInProgressDto); // سترجع قائمة فارغة إذا لم يوجد
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting currently reading books for user {userId}: {ex.ToString()}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }
    }
}