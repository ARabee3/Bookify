using Microsoft.AspNetCore.Mvc;
using Bookify.Entities; // أو DTOs لو الـ Service هترجع DTOs
using Bookify.Interfaces; // <<< مهمة جداً عشان ISummaryService
using System;
using System.Security.Claims; // <<< نحتاجها فقط لو AddBookSummaryAsync مفعلة
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization; // <<< نحتاجها فقط لو AddBookSummaryAsync مفعلة
using Microsoft.AspNetCore.Http; // <<< نحتاجها فقط لو AddBookSummaryAsync مفعلة
// using Bookify.Contexts; // لم نعد بحاجة للـ DbContext هنا مباشرة
// using Microsoft.EntityFrameworkCore; // لم نعد بحاجة لها هنا
// using System.Linq; // لم نعد بحاجة لها هنا
// using System.Collections.Generic; // الـ Service هي اللي بترجع List/IEnumerable

namespace Bookify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SummariesController : ControllerBase
    {
        // --- الاعتماد على الـ Service Interface ---
        private readonly ISummaryService _summaryService;

        // --- حقن الـ Service في الـ Constructor ---
        public SummariesController(ISummaryService summaryService)
        {
            _summaryService = summaryService;
        }

        /*
        // --- تم تعليق ميثود إضافة ملخص عام للكتاب مؤقتاً ---
        [Authorize]
        [HttpPost("ForBook/{bookId}")]
        public async Task<IActionResult> AddBookSummaryAsync(int bookId, [FromBody] string content)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) { return Unauthorized("User ID not found."); }

            if (string.IsNullOrWhiteSpace(content)) { return BadRequest("Summary content required."); }

            try
            {
                var createdSummary = await _summaryService.AddBookSummaryAsync(bookId, content, userId);
                return StatusCode(StatusCodes.Status201Created, createdSummary);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding summary: {ex.Message}"); // يفضل Logger
                return StatusCode(500, "An error occurred while adding summary.");
            }
        }
        */

        // --- Endpoint لجلب كل ملخصات الشابترات لكتاب معين ---
        // (تستدعي الـ Service) - متاحة للجميع
        [HttpGet("ByBook/{bookId}")]
        public async Task<IActionResult> GetChapterSummariesForBookAsync(int bookId)
        {
            try
            {
                var summaries = await _summaryService.GetChapterSummariesForBookAsync(bookId);
                return Ok(summaries);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting summaries for book {bookId}: {ex.Message}"); // Logger
                return StatusCode(500, "An error occurred while retrieving summaries.");
            }
        }

        // --- Endpoint لجلب ملخص شابتر معين ---
        // (تستدعي الـ Service) - متاحة للجميع
        [HttpGet("ByChapter/{chapterId}")]
        public async Task<IActionResult> GetSummaryForChapterAsync(int chapterId)
        {
            try
            {
                var summary = await _summaryService.GetSummaryForChapterAsync(chapterId);
                if (summary == null)
                {
                    return NotFound($"No summary found for chapter with ID {chapterId}.");
                }
                return Ok(summary);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting summary for chapter {chapterId}: {ex.Message}"); // Logger
                return StatusCode(500, "An error occurred while retrieving summary.");
            }
        }
    }
}