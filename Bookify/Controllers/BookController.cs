using Bookify.DTOs;
using Bookify.DTOs.Ai; // <<< مهمة جداً
using Bookify.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic; // <<< مهمة جداً
using System.ComponentModel;
using System.Linq;              // <<< مهمة جداً
using System.Threading.Tasks;

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
        [ProducesResponseType(typeof(PaginatedFilteredBooksDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllBooksAsync(
            [FromQuery, DefaultValue(1)] int pageNumber = 1,
            [FromQuery, DefaultValue(10)] int pageSize = 10)
        {
            try
            {
                // Basic validation
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;

                var paginatedBooks = await _bookService.GetAllBooksAsync(pageNumber, pageSize);
                return Ok(paginatedBooks);
            }
            catch (Exception ex)
            {
                // Use a real logger in production
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
            if (criteria.PageNumber < 1) criteria.PageNumber = 1;
            if (criteria.PageSize < 1) criteria.PageSize = 10;

            try
            {
                // 1. Get the FULL list of filtered books from the AI service
                var aiFilteredBooks = await _aiRecommendationService.GetFilteredBooksFromAiAsync(criteria);

                var emptyResponse = new PaginatedFilteredBooksDto
                {
                    TotalBooks = 0,
                    PageNumber = criteria.PageNumber,
                    PageSize = criteria.PageSize,
                    Books = new List<BookListItemDto>()
                };

                if (aiFilteredBooks == null || !aiFilteredBooks.Any())
                {
                    return Ok(emptyResponse);
                }

                // 2. Extract the titles to search for in our local database
                var filteredTitles = aiFilteredBooks
                                     .Select(b => b.Title)
                                     .Where(t => !string.IsNullOrEmpty(t))
                                     .Select(t => t!)
                                     .ToList();

                if (!filteredTitles.Any())
                {
                    return Ok(emptyResponse);
                }

                // 3. Get the full book details from our database
                var allBooksFromDb = await _bookService.GetBooksByTitlesAsync(filteredTitles);

                // 4. Apply pagination to the list we retrieved from OUR database
                var totalBooks = allBooksFromDb.Count;
                var paginatedBooks = allBooksFromDb
                                        .Skip((criteria.PageNumber - 1) * criteria.PageSize)
                                        .Take(criteria.PageSize)
                                        .ToList();

                // 5. Build the final paginated response object
                var finalResponse = new PaginatedFilteredBooksDto
                {
                    TotalBooks = totalBooks,
                    PageNumber = criteria.PageNumber,
                    PageSize = criteria.PageSize,
                    Books = paginatedBooks
                };

                return Ok(finalResponse);
            }
            catch (Exception ex)
            {
                // Assuming you have logging configured
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