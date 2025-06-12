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

        public async Task<IEnumerable<Summary>> GetSummariesByBookIdAsync(int bookId)
        {
            // نفس الكود اللي كان في Controller لجلب ملخصات الشابترات
            return await _context.Summaries
                             .Where(s => s.Chapter != null && s.Chapter.BookID == bookId)
                             .ToListAsync();
        }

        public async Task<Summary?> GetSummaryByChapterIdAsync(int chapterId)
        {
            // نفس الكود اللي كان في Controller لجلب ملخص شابتر واحد
            return await _context.Summaries
                             .FirstOrDefaultAsync(s => s.ChapterID == chapterId);
        }

        public async Task AddAsync(Summary summary)
        {
            await _context.Summaries.AddAsync(summary);
            // ملاحظة: SaveChangesAsync() المفروض تتم في الـ Service أو Unit of Work Pattern
            // لكن للتبسيط ممكن نخليها هنا مؤقتاً أو في الـ Service
            // await _context.SaveChangesAsync();
        }
    }
}