using Bookify.DTOs;
using Bookify.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System; // عشان Exception
using System.Collections.Generic; // عشان IEnumerable

namespace Bookify.Controllers
{
    [Route("api")] // ممكن نخلي المسار يبدأ بـ /api
    [ApiController]
    public class RatingsController : ControllerBase
    {
        private readonly IRatingService _ratingService;

        public RatingsController(IRatingService ratingService)
        {
            _ratingService = ratingService;
        }

        // --- Endpoint لإضافة تقييم ومراجعة لكتاب معين ---
        // POST /api/books/{bookId}/ratings
        [HttpPost("books/{bookId}/ratings")]
        [Authorize] // <<< لازم المستخدم يكون مسجل دخوله
        public async Task<IActionResult> AddBookRating(int bookId, [FromBody] AddRatingDto addRatingDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized("User ID not found in token.");
            }

            try
            {
                var createdRatingDto = await _ratingService.AddRatingAsync(userId, bookId, addRatingDto);

                if (createdRatingDto == null)
                {
                    // ممكن يكون الكتاب مش موجود أو المستخدم قيم قبل كده (حسب الـ Logic في الـ Service)
                    // ممكن نرجع 404 أو 409 بناءً على السبب اللي رجعه الـ Service
                    return BadRequest(new { Message = "Failed to add rating. Book not found or user already rated." });
                }
                // نرجع 201 Created مع الـ DTO بتاع التقييم اللي اتعمل وممكن لينك ليه
                return CreatedAtAction(nameof(GetRatingById), new { ratingId = createdRatingDto.RatingID }, createdRatingDto);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error adding rating for book {bookId} by user {userId}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while adding the rating.");
            }
        }

        // --- Endpoint لجلب كل التقييمات والمراجعات لكتاب معين (مع Pagination) ---
        // GET /api/books/{bookId}/ratings?pageNumber=1&pageSize=10
        [HttpGet("books/{bookId}/ratings")]
        public async Task<IActionResult> GetRatingsForBook(int bookId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            // ممكن نحدد Max PageSize عشان نحمي السيرفر

            try
            {
                var ratingsDto = await _ratingService.GetRatingsForBookAsync(bookId, pageNumber, pageSize);
                // لو ratingsDto رجعت فاضية، ده معناه مفيش تقييمات أو الكتاب مش موجود
                // الـ Service هي اللي بتتعامل مع حالة الكتاب مش موجود وبترجع قايمة فاضية
                return Ok(ratingsDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting ratings for book {bookId}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving ratings.");
            }
        }

        // --- Endpoint (اختياري) لجلب تقييم معين بالـ ID بتاعه ---
        // GET /api/ratings/{ratingId}
        // (ممكن نحتاجه لو عملنا CreatedAtAction في AddBookRating)
        [HttpGet("ratings/{ratingId}")]
        public async Task<IActionResult> GetRatingById(int ratingId)
        {
            // TODO: Implement this in IRatingService and RatingService
            // It should fetch a single rating by its RatingID and map it to RatingDto
            // var ratingDto = await _ratingService.GetRatingByIdAsync(ratingId);
            // if (ratingDto == null) return NotFound();
            // return Ok(ratingDto);
            return Ok(new { Message = "GetRatingById endpoint not fully implemented yet.", Id = ratingId }); // مؤقتاً
        }


        // --- Endpoint لتعديل تقييم موجود ---
        // PUT /api/ratings/{ratingId}
        [HttpPut("ratings/{ratingId}")]
        [Authorize] // <<< لازم المستخدم يكون مسجل وصاحب التقييم
        public async Task<IActionResult> UpdateBookRating(int ratingId, [FromBody] UpdateRatingDto updateRatingDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized("User ID not found in token.");
            }

            try
            {
                var updatedRatingDto = await _ratingService.UpdateRatingAsync(userId, ratingId, updateRatingDto);

                if (updatedRatingDto == null)
                {
                    // ممكن يكون التقييم مش موجود أو المستخدم مش صاحبه
                    return Forbid("You are not allowed to update this rating or rating not found."); // أو 404 أو 403
                }
                return Ok(updatedRatingDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating rating {ratingId} by user {userId}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the rating.");
            }
        }

        // --- Endpoint لحذف تقييم ---
        // DELETE /api/ratings/{ratingId}
        [HttpDelete("ratings/{ratingId}")]
        [Authorize] // <<< لازم المستخدم يكون مسجل وصاحب التقييم (أو Admin)
        public async Task<IActionResult> DeleteBookRating(int ratingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized("User ID not found in token.");
            }

            try
            {
                var success = await _ratingService.DeleteRatingAsync(userId, ratingId);
                if (!success)
                {
                    // التقييم مش موجود أو المستخدم مش صاحبه
                    return Forbid("You are not allowed to delete this rating or rating not found."); // أو 404 أو 403
                }
                return NoContent(); // 204 No Content (معناها تم الحذف بنجاح ومفيش حاجة نرجعها)
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting rating {ratingId} by user {userId}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the rating.");
            }
        }
    }
}