using Microsoft.AspNetCore.Mvc;
using Bookify.Entities; // أو DTOs لو الـ Service سترجع DTOs لاحقاً
using Bookify.Interfaces; // <<< مهمة جداً عشان IBookService
using System;
using System.Threading.Tasks;
// using Microsoft.AspNetCore.Http; // لم تعد مستخدمة هنا حالياً
// using Bookify.DTOs.Ai; // لم تعد مستخدمة هنا
// using System.Collections.Generic; // لم تعد مستخدمة هنا
// using System.Linq; // لم تعد مستخدمة هنا

namespace Bookify.Controllers
{
    [Route("api/[controller]")] // المسار الأساسي: /api/books
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;
        // تم إزالة IAiRecommendationService من هنا

        // --- تم تعديل الـ Constructor ليحقن IBookService فقط ---
        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }

        // --- Endpoint لجلب كل الكتب (مع إمكانية الفلترة بالـ Category) ---
        // (بقيت كما هي)
        [HttpGet]
        public async Task<IActionResult> GetAllBooksAsync(string? category = null)
        {
            try
            {
                var books = await _bookService.GetAllBooksAsync(category);
                return Ok(books);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting books: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving books.");
            }
        }

        // --- Endpoint لجلب كتاب واحد بالـ ID بتاعه ---
        // (بقيت كما هي)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookByIdAsync(int id)
        {
            try
            {
                var book = await _bookService.GetBookByIdAsync(id);
                if (book == null)
                {
                    return NotFound($"Book with ID {id} not found.");
                }
                return Ok(book);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting book {id}: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving the book.");
            }
        }

        // --- تم حذف كل الـ Endpoints الخاصة بالـ Recommendation من هنا ---
        // GetAiRankBasedRecommendationsAsync
        // GetAiFilteredBooksAsync
        // GetAiContentBasedRecommendationsAsync


        /*
        // --- ميثود إضافة كتاب (لا تزال معلقة) ---
        [HttpPost]
        // [Authorize]
        public async Task<IActionResult> AddBookAsync(/* ... DTO or parameters ... */
        /* {
             throw new NotImplementedException();
        } */
        
    }
}