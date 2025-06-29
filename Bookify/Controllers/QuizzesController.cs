using Microsoft.AspNetCore.Mvc;
using Bookify.Contexts;
using Bookify.Entities;
using Bookify.DTOs;
using Bookify.Interfaces; // <<< Add this
using Microsoft.AspNetCore.Authorization; // <<< Add this
using System.Threading.Tasks; // <<< Add this

namespace Bookify.Controllers
{
    [Route("api")] // Change route to be more RESTful
    [ApiController]
    public class QuizzesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IBookProcessingService _bookProcessingService; // <<< Add this

        public QuizzesController(AppDbContext context, IBookProcessingService bookProcessingService) // <<< Modify constructor
        {
            _context = context;
            _bookProcessingService = bookProcessingService; // <<< Add this
        }

        // --- NEW ENDPOINT FOR ON-THE-FLY QUIZ GENERATION ---
        [HttpGet("chapters/{chapterId}/quiz")]
        [Authorize]
        public async Task<IActionResult> GenerateQuizForChapter(int chapterId)
        {
            try
            {
                var quizDto = await _bookProcessingService.GenerateQuizForChapterAsync(chapterId);
                if (quizDto == null)
                {
                    return NotFound("Could not generate a quiz for this chapter. The chapter may not exist or the AI service failed.");
                }
                return Ok(quizDto);
            }
            catch (System.Collections.Generic.KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (System.IO.FileNotFoundException ex)
            {
                // Do not expose file paths to the client
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Book content is missing on the server." });
            }
            catch (Exception ex)
            {
                // Log the full exception ex.ToString()
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred while generating the quiz." });
            }
        }

        // --- OLD METHOD (Can be kept or removed) ---
        [HttpPost("quizzes")] // Kept old route for compatibility
        public async Task<IActionResult> AddQuiz([FromBody] Quiz quiz) // Simplified for example
        {
            await _context.Quizzes.AddAsync(quiz);
            await _context.SaveChangesAsync();
            return Ok("Quiz added successfully!");
        }
    }
}