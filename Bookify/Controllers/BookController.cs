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

        public BooksController(
            IBookService bookService,
            IAiRecommendationService aiRecommendationService)
        {
            _bookService = bookService;
            _aiRecommendationService = aiRecommendationService;
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

        // --- Endpoint لجلب كتاب واحد بالـ ID ---
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookByIdAsync(int id)
        {
            try
            {
                var bookDetail = await _bookService.GetBookByIdAsync(id);
                if (bookDetail == null)
                {
                    return NotFound($"Book with ID {id} not found.");
                }
                return Ok(bookDetail);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting book {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the book.");
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