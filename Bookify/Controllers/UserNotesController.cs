using Bookify.DTOs;
using Bookify.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using System.Linq; // <<< مهمة لـ Any()
using System.Collections.Generic; // <<< مهمة لـ List

namespace Bookify.Controllers
{
    [Route("api/[controller]")] // المسار هيبقى /api/UserNotes
    [ApiController]
    [Authorize] // كل الـ Endpoints هنا مؤمنة
    public class UserNotesController : ControllerBase
    {
        private readonly IUserNoteService _noteService;

        public UserNotesController(IUserNoteService noteService)
        {
            _noteService = noteService;
        }

        // POST /api/usernotes
        [HttpPost]
        public async Task<IActionResult> CreateNote([FromBody] CreateNoteDto createNoteDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // userId لا يمكن أن يكون null هنا بسبب [Authorize]، لكن يمكن التحقق كخطوة إضافية
            if (userId == null) return Unauthorized("User ID cannot be determined.");


            try
            {
                var noteDto = await _noteService.CreateNoteAsync(userId, createNoteDto);
                if (noteDto == null)
                {
                    return BadRequest(new { Message = "Failed to create note. Ensure BookID or ChapterID is valid if provided, and content is not empty." });
                }
                // استخدام nameof(GetNoteById) لإنشاء الـ Location header
                return CreatedAtAction(nameof(GetNoteById), new { noteId = noteDto.NoteID }, noteDto);
            }
            catch (ArgumentException ex) // لو الـ Service رمت ArgumentException
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                // Log the exception (ex.ToString())
                Console.WriteLine($"Error creating note for user {userId}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred while creating the note." });
            }
        }

        // GET /api/usernotes/{noteId}
        [HttpGet("{noteId}")]
        public async Task<IActionResult> GetNoteById(int noteId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var note = await _noteService.GetNoteByIdAsync(userId, noteId);
            if (note == null)
            {
                return NotFound(new { Message = $"Note with ID {noteId} not found or you do not have permission to view it." });
            }
            return Ok(note);
        }

        // GET /api/usernotes/book/{bookId}
        [HttpGet("book/{bookId}")]
        public async Task<IActionResult> GetNotesForBook(int bookId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var notes = await _noteService.GetMyNotesForBookAsync(userId, bookId);
            return Ok(notes); // سترجع قائمة فارغة إذا لم توجد ملاحظات
        }

        // --- Endpoints لباقي الميثودات في الـ Service ---

        // GET /api/usernotes/chapter/{chapterId}
        [HttpGet("chapter/{chapterId}")]
        public async Task<IActionResult> GetNotesForChapter(int chapterId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();
            var notes = await _noteService.GetMyNotesForChapterAsync(userId, chapterId);
            return Ok(notes);
        }

        // GET /api/usernotes/my
        [HttpGet("my")]
        public async Task<IActionResult> GetAllMyNotes()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();
            var notes = await _noteService.GetAllMyNotesAsync(userId);
            return Ok(notes);
        }

        // PUT /api/usernotes/{noteId}
        [HttpPut("{noteId}")]
        public async Task<IActionResult> UpdateNote(int noteId, [FromBody] UpdateNoteDto updateNoteDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var success = await _noteService.UpdateNoteAsync(userId, noteId, updateNoteDto);
            if (!success)
            {
                return NotFound(new { Message = $"Note with ID {noteId} not found or you do not have permission to update it." });
            }
            return NoContent(); // أو Ok مع رسالة نجاح
        }

        // DELETE /api/usernotes/{noteId}
        [HttpDelete("{noteId}")]
        public async Task<IActionResult> DeleteNote(int noteId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var success = await _noteService.DeleteNoteAsync(userId, noteId);
            if (!success)
            {
                return NotFound(new { Message = $"Note with ID {noteId} not found or you do not have permission to delete it." });
            }
            return NoContent(); // أو Ok مع رسالة نجاح
        }
    }
}