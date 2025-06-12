using Bookify.DTOs;
using Bookify.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System; // عشان Exception و KeyNotFoundException
using Microsoft.AspNetCore.Http; // عشان StatusCodes

namespace Bookify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // <<< كل الـ Endpoints هنا مؤمنة
    public class ProgressController : ControllerBase
    {
        private readonly IProgressService _progressService;

        public ProgressController(IProgressService progressService)
        {
            _progressService = progressService;
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProgress([FromBody] UpdateProgressDto progressDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized("User not identified.");

            try
            {
                var resultDto = await _progressService.UpdateOrCreateUserProgressAsync(userId, progressDto);
                if (resultDto == null)
                {
                    // الـ Service ممكن ترجع null لو فيه مشكلة منطقية بسيطة لم يتم التعامل معها كـ Exception
                    return BadRequest("Could not update progress. Please check input data.");
                }
                return Ok(resultDto);
            }
            catch (KeyNotFoundException knfex) // لو الكتاب مش موجود
            {
                return NotFound(new { message = knfex.Message });
            }
            catch (ArgumentException argex) // لو الشابتر مش تبع الكتاب
            {
                return BadRequest(new { message = argex.Message });
            }
            catch (Exception ex) // للأخطاء العامة
            {
                // Log the exception (ex)
                Console.WriteLine($"Error in UpdateProgress: {ex}"); // استبدل بـ Logger حقيقي
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while updating progress.");
            }
        }

        [HttpGet("book/{bookId}")]
        public async Task<IActionResult> GetBookProgress(int bookId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized("User not identified.");

            var progressDto = await _progressService.GetUserProgressForBookAsync(userId, bookId);
            if (progressDto == null)
            {
                // نرجع DTO افتراضي يوضح إن التقدم لم يبدأ بعد
                return Ok(new ProgressDto
                {
                    BookID = bookId,
                    CompletionPercentage = 0,
                    Status = Entities.CompletionStatus.NotStarted
                });
            }
            return Ok(progressDto);
        }

        [HttpGet("my/all")]
        public async Task<IActionResult> GetAllMyProgress()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized("User not identified.");

            var progressesDto = await _progressService.GetAllUserProgressAsync(userId);
            return Ok(progressesDto);
        }

        [HttpGet("my/currently-reading")]
        public async Task<IActionResult> GetMyCurrentlyReading()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized("User not identified.");

            var booksInProgressDto = await _progressService.GetCurrentlyReadingBooksAsync(userId);
            return Ok(booksInProgressDto);
        }
    }
}