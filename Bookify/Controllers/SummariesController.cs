using Microsoft.AspNetCore.Mvc;
using Bookify.Interfaces;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System;
using Microsoft.AspNetCore.Http;

namespace Bookify.Controllers
{
    [Route("api/[controller]")] // المسار: /api/Summaries
    [ApiController]
    public class SummariesController : ControllerBase
    {
        private readonly ISummaryService _summaryService;

        public SummariesController(ISummaryService summaryService)
        {
            _summaryService = summaryService;
        }

        [HttpGet("book/{bookId}")]
        public async Task<IActionResult> GetSummariesForBook(int bookId)
        {
            var summaries = await _summaryService.GetSummariesForBookAsync(bookId);
            return Ok(summaries);
        }

        [HttpGet("chapter/{chapterId}")]
        public async Task<IActionResult> GetSummaryForChapter(int chapterId)
        {
            var summary = await _summaryService.GetSummaryForChapterAsync(chapterId);
            if (summary is null) // <<< استخدام is null أكثر أماناً
            {
                return NotFound($"No summary found for chapter with ID {chapterId}.");
            }
            return Ok(summary);
        }

        [HttpPost("generate/chapter/{chapterId}")]
        [Authorize] // Admin-only?
        public async Task<IActionResult> GenerateSummary(int chapterId)
        {
            try
            {
                var summaryDto = await _summaryService.GenerateAndSaveSummaryForChapterAsync(chapterId);
                return Ok(summaryDto); // نرجع Ok مع الملخص الجديد/المحدث
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error generating summary for chapter {chapterId}: {ex.Message}");
                // نرجع رسالة خطأ مناسبة للـ Frontend
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
    }
}