using Bookify.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bookify.Controllers
{
    [Route("api/book-processing")]
    [ApiController]
    [Authorize]
    public class BookProcessingController : ControllerBase
    {
        private readonly IBookProcessingService _bookProcessingService;
        private readonly ILogger<BookProcessingController> _logger;

        public BookProcessingController(IBookProcessingService bookProcessingService, ILogger<BookProcessingController> logger)
        {
            _bookProcessingService = bookProcessingService;
            _logger = logger;
        }

        [HttpPost("process-upload")]
        [RequestSizeLimit(100_000_000)] // 100 MB limit
        [RequestFormLimits(MultipartBodyLengthLimit = 100_000_000)]
        public async Task<IActionResult> ProcessUploadedBook(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file provided.");
            if (Path.GetExtension(file.FileName).ToLower() != ".pdf") return BadRequest("Only PDF files are allowed.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            _logger.LogInformation("Starting book processing for user {UserId} and file {FileName}", userId, file.FileName);

            try
            {
                var resultDto = await _bookProcessingService.ProcessUploadedBookAsync(file, userId);
                if (resultDto == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to process the uploaded book.");
                }
                // Use the route name from the original BooksController to point to the new book's resource URL
                return CreatedAtAction(nameof(BooksController.GetBookByIdAsync), "Books", new { id = resultDto.BookID }, resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing book for user {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred during book processing.");
            }
        }
    }
}