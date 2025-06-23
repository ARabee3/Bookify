using Bookify.Contexts;
using Bookify.Entities;
using Bookify.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Bookify.Repositories
{
    public class ChapterRepository : IChapterRepository
    {
        private readonly AppDbContext _context;

        public ChapterRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Chapter?> GetByIdAsync(int chapterId)
        {
            // عادةً لما بنجيب شابتر بالـ ID، ممكن نحتاج معاه معلومات الكتاب
            return await _context.Chapters
                                 .Include(c => c.Book) // <<< جلب الكتاب مع الشابتر
                                 .FirstOrDefaultAsync(c => c.ChapterID == chapterId);
        }

        public async Task<bool> ChapterExistsAndBelongsToBookAsync(int chapterId, int bookId)
        {
            return await _context.Chapters.AnyAsync(c => c.ChapterID == chapterId && c.BookID == bookId);
        }
    }
}