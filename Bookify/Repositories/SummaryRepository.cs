using Bookify.Contexts;
using Bookify.Entities;
using Bookify.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bookify.Repositories
{
    public class SummaryRepository : ISummaryRepository
    {
        private readonly AppDbContext _context;

        public SummaryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Summary summary)
        {
            await _context.Summaries.AddAsync(summary);
        }

        public async Task<IEnumerable<Summary>> GetSummariesForBookAsync(int bookId)
        {
            return await _context.Summaries
                                 .Where(s => s.BookID == bookId)
                                 .Include(s => s.Book) // لتضمين بيانات الكتاب
                                 .Include(s => s.Chapter) // لتضمين بيانات الشابتر
                                 .OrderBy(s => s.ChapterID) // أو CreatedAt
                                 .ToListAsync();
        }

        public async Task<Summary?> GetSummaryForChapterAsync(int chapterId)
        {
            return await _context.Summaries
                                 .Where(s => s.ChapterID == chapterId)
                                 .Include(s => s.Book)
                                 .Include(s => s.Chapter)
                                 .FirstOrDefaultAsync();
        }
    }
}