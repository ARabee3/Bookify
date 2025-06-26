using Bookify.DTOs;
using Bookify.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bookify.Controllers
{
    [Route("api/[controller]")] // المسار: /api/mylibrary
    [ApiController]
    [Authorize] // كل الـ Endpoints هنا مؤمنة
    public class MyLibraryController : ControllerBase
    {
        private readonly IUserLibraryService _userLibraryService;

        public MyLibraryController(IUserLibraryService userLibraryService)
        {
            _userLibraryService = userLibraryService;
        }

        // POST /api/mylibrary/books/{bookId} (لإضافة كتاب للمكتبة)
        [HttpPost("books/{bookId}")]
        public async Task<IActionResult> AddBookToMyLibrary(int bookId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized("User ID not found.");

            try
            {
                var success = await _userLibraryService.AddBookToMyLibraryAsync(userId, bookId);
                if (!success)
                {
                    // قد يكون الكتاب غير موجود أو هناك خطأ آخر
                    return BadRequest(new { Message = $"Failed to add book ID {bookId} to library. Book might not exist." });
                }
                return StatusCode(StatusCodes.Status201Created, new { Message = $"Book ID {bookId} added to your library." });
            }
            catch (DbUpdateException ex) // للتعامل مع خطأ الـ Composite Key لو حاول يضيف كتاب موجود
            {
                Console.WriteLine($"DbUpdateException when adding book {bookId} for user {userId}: {ex.InnerException?.Message ?? ex.Message}");
                return Conflict(new { Message = $"Book ID {bookId} is already in your library." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding book {bookId} to library for user {userId}: {ex.ToString()}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        // DELETE /api/mylibrary/books/{bookId} (لإزالة كتاب من المكتبة)
        [HttpDelete("books/{bookId}")]
        public async Task<IActionResult> RemoveBookFromMyLibrary(int bookId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized("User ID not found.");

            try
            {
                var success = await _userLibraryService.RemoveBookFromMyLibraryAsync(userId, bookId);
                if (!success)
                {
                    return NotFound(new { Message = $"Book ID {bookId} not found in your library." });
                }
                return NoContent(); // 204 No Content
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing book {bookId} from library for user {userId}: {ex.ToString()}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        // GET /api/mylibrary/books (لجلب كتب مكتبة المستخدم مع Pagination)
        [HttpGet("books")]
        public async Task<IActionResult> GetMyLibraryBooks([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 50) pageSize = 50; // حد أقصى لحجم الصفحة

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized("User ID not found.");

            try
            {
                var books = await _userLibraryService.GetMyLibraryBooksAsync(userId, pageNumber, pageSize);
                // يمكن إضافة معلومات الـ Pagination للـ Header هنا لو الـ Frontend محتاجها
                return Ok(books);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting library books for user {userId}: {ex.ToString()}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        // GET /api/mylibrary/books/{bookId}/status (للتحقق إذا كان الكتاب في مكتبة المستخدم)
        [HttpGet("books/{bookId}/status")]
        public async Task<IActionResult> CheckBookInMyLibrary(int bookId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized("User ID not found.");

            try
            {
                var isInLibrary = await _userLibraryService.CheckIfBookInMyLibraryAsync(userId, bookId);
                return Ok(new { isInLibrary = isInLibrary });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking book {bookId} in library for user {userId}: {ex.ToString()}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }
    }
}