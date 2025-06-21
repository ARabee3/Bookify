using Microsoft.AspNetCore.Mvc;
using Bookify.DTOs;
using Bookify.DTOs.Ai; // <<< مهمة جداً
using Bookify.Interfaces;
using System;
using System.Collections.Generic; // <<< مهمة جداً
using System.Linq;              // <<< مهمة جداً
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Bookify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;
        private readonly IAiRecommendationService _aiRecommendationService; // <<< تم إضافتها

        public BooksController(
            IBookService bookService,
            IAiRecommendationService aiRecommendationService) // <<< تم إضافتها
        {
            _bookService = bookService;
            _aiRecommendationService = aiRecommendationService; // <<< تم إضافتها
        }

        // --- Endpoint لجلب كل الكتب ---
        [HttpGet]
        public async Task<IActionResult> GetAllBooksAsync(string? category = null)
        {
            try
            {
                var books = await _bookService.GetAllBooksAsync(category);
                return Ok(books); // الآن ترجع List<BookListItemDto>
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting books: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving books.");
            }
        }

        // --- Endpoint لجلب كتاب واحد بالـ ID بتاعه ---
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookByIdAsync(int id)
        {
            try
            {
                var bookDetail = await _bookService.GetBookByIdAsync(id); // currentUserId اختياري
                if (bookDetail == null)
                {
                    return NotFound($"Book with ID {id} not found.");
                }
                return Ok(bookDetail); // الآن ترجع BookDetailDto
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting book {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the book.");
            }
        }

        // --- بداية Endpoints التوصيات الجديدة ---

        // GET /api/books/recommendations/top-ranked?weightViews=0.3&topN=5
        [HttpGet("recommendations/top-ranked")]
        public async Task<IActionResult> GetAiRankBasedRecommendationsAsync(
            [FromQuery] float? weightViews = null,
            [FromQuery] float? weightRating = null,
            [FromQuery] int? topN = null)
        {
            try
            {
                var aiRecommendedBooks = await _aiRecommendationService.GetRankBasedRecommendationsAsync(weightViews, weightRating, topN);
                if (aiRecommendedBooks == null || !aiRecommendedBooks.Any())
                {
                    return Ok(new List<BookListItemDto>());
                }

                var recommendedTitles = aiRecommendedBooks
                                        .Select(b => b.Title)
                                        .Where(t => !string.IsNullOrEmpty(t))
                                        .Select(t => t!)
                                        .ToList();
                if (!recommendedTitles.Any())
                {
                    return Ok(new List<BookListItemDto>());
                }

                var booksFromOurDb = await _bookService.GetBooksByTitlesAsync(recommendedTitles);
                return Ok(booksFromOurDb);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting AI rank-based recommendations: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching rank-based recommendations.");
            }
        }

        // GET /api/books/recommendations/filter?genre=History&difficulty=Beginner
        [HttpGet("recommendations/filter")]
        public async Task<IActionResult> GetAiFilteredBooksAsync([FromQuery] FilterCriteriaDto criteria)
        {
            try
            {
                var aiFilteredBooks = await _aiRecommendationService.GetFilteredBooksFromAiAsync(criteria);
                if (aiFilteredBooks == null || !aiFilteredBooks.Any())
                {
                    return Ok(new List<BookListItemDto>());
                }

                var filteredTitles = aiFilteredBooks
                                     .Select(b => b.Title)
                                     .Where(t => !string.IsNullOrEmpty(t))
                                     .Select(t => t!)
                                     .ToList();
                if (!filteredTitles.Any())
                {
                    return Ok(new List<BookListItemDto>());
                }

                var booksFromOurDb = await _bookService.GetBooksByTitlesAsync(filteredTitles);
                return Ok(booksFromOurDb);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting AI filtered books: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching filtered books.");
            }
        }

        // GET /api/books/{id}/recommendations/content
        [HttpGet("{id}/recommendations/content")]
        public async Task<IActionResult> GetAiContentBasedRecommendationsAsync(int id, [FromQuery] int? topN = null)
        {
            try
            {
                var ourBook = await _bookService.GetBookByIdAsync(id);
                if (ourBook == null || string.IsNullOrWhiteSpace(ourBook.Title))
                {
                    return NotFound($"Book with ID {id} not found or has no title in our database.");
                }

                var aiRecommendationResponse = await _aiRecommendationService.GetContentBasedRecommendationsAsync(ourBook.Title, topN);
                if (aiRecommendationResponse == null || aiRecommendationResponse.Recommendations == null || !aiRecommendationResponse.Recommendations.Any())
                {
                    return Ok(new { inputBookTitle = ourBook.Title, recommendedBooks = new List<BookListItemDto>() });
                }

                var recommendedBookTitles = aiRecommendationResponse.Recommendations
                                                .Where(t => !string.IsNullOrEmpty(t))
                                                .Select(t => t!)
                                                .ToList();
                if (!recommendedBookTitles.Any())
                {
                    return Ok(new { inputBookTitle = ourBook.Title, recommendedBooks = new List<BookListItemDto>() });
                }
                var booksFromOurDb = await _bookService.GetBooksByTitlesAsync(recommendedBookTitles);

                return Ok(new
                {
                    inputBookTitle = aiRecommendationResponse.InputTitle,
                    recommendedBooks = booksFromOurDb
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting AI content-based recommendations for book ID {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching content-based recommendations.");
            }
        }

        /*
        // AddBookAsync (معلقة)
        */
    }
}