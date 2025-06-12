using Microsoft.AspNetCore.Mvc;
using Bookify.Contexts;
using Bookify.Entities;
using Bookify.Contexts;
using Bookify.Entities;
using Bookify.DTOs;

namespace Bookify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizzesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public QuizzesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        // استخدم async Task<IActionResult> عشان الأداء يبقى أفضل
        public async Task<IActionResult> AddQuiz(int bookId, int? chapterId, bool isFinal, List<QuestionDto> questions)
        {
            // --- تحقق مبدئي (مثال) ---
            if (questions == null || !questions.Any())
            {
                return BadRequest("Quiz must have at least one question.");
            }
            // TODO: ممكن تتحقق لو الـ bookId أو chapterId موجودين فعلاً في الداتابيز

            var quiz = new Quiz
            {
                BookID = bookId,
                ChapterID = chapterId,
                IsFinal = isFinal,
                CreatedDate = DateTime.UtcNow // الأفضل استخدام UtcNow للسيرفر
            };

            // --- الجزء ده هيتغير ---
            foreach (var questionDto in questions)
            {
                if (questionDto.Answers == null || !questionDto.Answers.Any())
                {
                    // ممكن تتجاهل السؤال ده أو ترجع خطأ
                    continue; // نتجاهله مؤقتاً
                }

                var newQuestion = new Question
                {
                    Text = questionDto.Text,
                    Quiz = quiz // اربط السؤال بالكويز مباشرة هنا أسهل
                };

                // لوب على الإجابات اللي جاية في الـ DTO
                foreach (var answerDto in questionDto.Answers)
                {
                    newQuestion.Answers.Add(new Answer
                    {
                        Text = answerDto.Text,
                        IsCorrect = answerDto.IsCorrect,
                        Question = newQuestion // اربط الإجابة بالسؤال الجديد
                    });
                }
                // مش محتاجين نضيف newQuestion لـ quiz.Questions هنا
                // لأن EF Core هيعرف إنها مرتبطة بالكويز لما نضيف الكويز نفسه
                // وهو هيضيف الأسئلة والإجابات بالتبعية (Cascade)
            }
            // --- نهاية الجزء المتغير ---


            // استخدام async methods
            await _context.Quizzes.AddAsync(quiz);
            await _context.SaveChangesAsync();

            // الأفضل نرجع الكويز اللي اتعمل أو لينك ليه بدل مجرد رسالة
            // return CreatedAtAction(nameof(GetQuizById), new { id = quiz.QuizID }, quiz); // (محتاج تعمل ميثود GetQuizById الأول)
            return Ok("Quiz added successfully!"); // أو نكتفي بدي مؤقتاً
        }
    }

    
}