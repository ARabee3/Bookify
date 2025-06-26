using Bookify.Contexts;
using Bookify.Entities;
using Bookify.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bookify.Repositories
{
    public class ProgressRepository : IProgressRepository
    {
        private readonly AppDbContext _context;

        public ProgressRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Progress?> GetUserProgressForBookAsync(string userId, int bookId)
        {
            return await _context.Progresses
                                 .Include(p => p.Book) // Book مهم للـ DTO
                                                       // .Include(p => p.LastReadChapter) // <<< تم الحذف
                                 .FirstOrDefaultAsync(p => p.UserID == userId && p.BookID == bookId);
        }

        public async Task<IEnumerable<Progress>> GetAllUserProgressAsync(string userId)
        {
            return await _context.Progresses
                                 .Where(p => p.UserID == userId)
                                 .Include(p => p.Book)
                                 // .Include(p => p.LastReadChapter) // <<< تم الحذف
                                 .OrderByDescending(p => p.LastUpdatedAt)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<Progress>> GetCurrentlyReadingProgressAsync(string userId)
        {
            return await _context.Progresses
                                .Where(p => p.UserID == userId && p.Status == CompletionStatus.InProgress)
                                .Include(p => p.Book)
                                .OrderByDescending(p => p.LastUpdatedAt)
                                .ToListAsync();
        }

        public async Task AddAsync(Progress progress) // تم تعديل الاسم
        {
            await _context.Progresses.AddAsync(progress);
        }

        public void Update(Progress progress) // تم تعديل الاسم
        {
            _context.Progresses.Update(progress);
        }
    }
}