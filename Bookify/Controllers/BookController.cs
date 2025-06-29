using Bookify.DTOs;
using Bookify.DTOs.Ai;
using Bookify.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims; // لإضافة ClaimsTypes
using System.Threading.Tasks;

namespace Bookify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;
        private readonly IAiRecommendationService _aiRecommendationService;
        private readonly IBookProcessingService _bookProcessingService;
        private readonly ILogger<BooksController> _logger;

        public BooksController(
            IBookService bookService,
            IAiRecommendationService aiRecommendationService,
            IBookProcessingService bookProcessingService,
            ILogger<BooksController> logger
            

            )
        {
            _bookService = bookService;
            _aiRecommendationService = aiRecommendationService;
            _bookProcessingService = bookProcessingService;
            _logger = logger;
        }

        // --- Endpoint لجلب الكتب مع فلترة و Pagination ---
        [HttpGet]
        public async Task<IActionResult> GetAllBooksAsync([FromQuery] BookFilterDto filter)
        {
            try
            {
                var paginatedBooks = await _bookService.GetAllBooksAsync(filter);
                return Ok(paginatedBooks);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting books: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving books.");
            }
        }

        [HttpGet("{id:int}")] // Make sure the :int constraint is here!
        [ActionName(nameof(GetBookByIdAsync))] // Explicitly name the action for CreatedAtAction
        public async Task<IActionResult> GetBookByIdAsync(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var book = await _bookService.GetBookByIdAsync(id, currentUserId);
            if (book == null)
            {
                return NotFound();
            }
            return Ok(book);
        }
        [HttpPost("process-upload")]
        [Authorize]
        [RequestSizeLimit(100_000_000)]
        public async Task<IActionResult> ProcessUploadedBook(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file provided.");
            if (Path.GetExtension(file.FileName).ToLower() != ".pdf") return BadRequest("Only PDF files are allowed.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized(); // Should be handled by [Authorize] but good practice

            _logger.LogInformation("Starting book processing for user {UserId} and file {FileName}", userId, file.FileName);

            try
            {
                var resultDto = await _bookProcessingService.ProcessUploadedBookAsync(file, userId);
                if (resultDto == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to process the uploaded book.");
                }

                // This now works because we are in the same controller as GetBookByIdAsync
                return CreatedAtAction(nameof(GetBookByIdAsync), new { id = resultDto.BookID }, resultDto);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error processing book for user {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred during book processing.");
            }
        }
        // --- Endpoint لرفع كتاب ومعالجته ---
        [HttpPost("upload")]
        [Authorize]
        [RequestSizeLimit(100_000_000)] // حد أقصى لحجم الملف (100MB) - اضبطه حسب الحاجة
        [RequestFormLimits(MultipartBodyLengthLimit = 100_000_000)]
        public async Task<IActionResult> UploadBook(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file provided.");
            if (Path.GetExtension(file.FileName).ToLower() != ".pdf") return BadRequest("Only PDF files are allowed.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            try
            {
                var resultDto = await _bookService.UploadAndProcessBookAsync(file, userId);
                if (resultDto == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to process the uploaded book.");
                }
                return CreatedAtAction(nameof(GetBookByIdAsync), new { id = resultDto.BookID }, resultDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading book for user {userId}: {ex.ToString()}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred during book processing.");
            }
        }


        // --- بداية Endpoints التوصيات (تعتمد على AI API) ---
        [HttpGet("recommendations/top-ranked")]
        public async Task<IActionResult> GetAiRankBasedRecommendationsAsync(
            [FromQuery] float? weightViews = null,
            [FromQuery] float? weightRating = null,
            [FromQuery] int? topN = null)
        {
            // ... (الكود كما هو من قبل)
            var aiBooks = await _aiRecommendationService.GetRankBasedRecommendationsAsync(weightViews, weightRating, topN);
            if (aiBooks == null || !aiBooks.Any()) return Ok(new List<BookListItemDto>());
            var titles = aiBooks.Select(b => b.Title).Where(t => t != null).Select(t => t!).ToList();
            var booksFromDb = await _bookService.GetBooksByTitlesAsync(titles);
            return Ok(booksFromDb);
        }

        [HttpGet("recommendations/filter")]
        public async Task<IActionResult> GetAiFilteredBooksAsync([FromQuery] FilterCriteriaDto criteria)
        {
            // ... (الكود كما هو من قبل)
            var aiBooks = await _aiRecommendationService.GetFilteredBooksFromAiAsync(criteria);
            if (aiBooks == null || !aiBooks.Any()) return Ok(new List<BookListItemDto>());
            var titles = aiBooks.Select(b => b.Title).Where(t => t != null).Select(t => t!).ToList();
            var booksFromDb = await _bookService.GetBooksByTitlesAsync(titles);
            return Ok(booksFromDb);
        }

        [HttpGet("{id}/recommendations/content")]
        public async Task<IActionResult> GetAiContentBasedRecommendationsAsync(int id, [FromQuery] int? topN = null)
        {
            // ... (الكود كما هو من قبل)
            var ourBook = await _bookService.GetBookByIdAsync(id);
            if (ourBook?.Title == null) return NotFound();
            var aiResponse = await _aiRecommendationService.GetContentBasedRecommendationsAsync(ourBook.Title, topN);
            if (aiResponse?.Recommendations == null || !aiResponse.Recommendations.Any()) return Ok(new { inputBookTitle = ourBook.Title, recommendedBooks = new List<BookListItemDto>() });
            var booksFromDb = await _bookService.GetBooksByTitlesAsync(aiResponse.Recommendations);
            return Ok(new { inputBookTitle = aiResponse.InputTitle, recommendedBooks = booksFromDb });
        }
    }
}