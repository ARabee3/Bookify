using Bookify.Contexts;
using Bookify.Entities;
using Bookify.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic; // <<< لإضافة IEnumerable
using System.Linq; // <<< لإضافة Where
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
            return await _context.Chapters.FirstOrDefaultAsync(c => c.ChapterID == chapterId);
        }

        public async Task<bool> ChapterExistsAndBelongsToBookAsync(int chapterId, int bookId)
        {
            return await _context.Chapters.AnyAsync(c => c.ChapterID == chapterId && c.BookID == bookId);
        }

        public async Task AddAsync(Chapter chapter) // <<< تم إضافتها
        {
            await _context.Chapters.AddAsync(chapter);
        }

        public async Task<IEnumerable<Chapter>> GetChaptersByBookIdAsync(int bookId) // <<< تم إضافتها
        {
            return await _context.Chapters.Where(c => c.BookID == bookId).OrderBy(c => c.ChapterNumber).ToListAsync();
        }
    }
}