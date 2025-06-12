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
                                 .FirstOrDefaultAsync(p => p.UserID == userId && p.BookID == bookId);
        }

        public async Task<IEnumerable<Progress>> GetAllUserProgressAsync(string userId)
        {
            return await _context.Progresses
                                 .Where(p => p.UserID == userId)
                                 .Include(p => p.Book) // مهم عشان نجيب تفاصيل الكتاب مع التقدم
                                                       // .Include(p => p.LastReadChapter) // لو حابب تجيب تفاصيل الشابتر كمان
                                 .OrderByDescending(p => p.StartDate) // نرتب بالأحدث
                                 .ToListAsync();
        }

        public async Task AddProgressAsync(Progress progress)
        {
            await _context.Progresses.AddAsync(progress);
            // لا يوجد SaveChanges هنا، الـ Service هي المسؤولة
        }

        public Task UpdateProgressAsync(Progress progress)
        {
            // EF Core بيعمل Track للتغييرات، مجرد تعديل الـ entity في الـ Service كافي
            // لكن لو عايز تكون صريح، ممكن تعمل كده:
            _context.Progresses.Update(progress);
            // أو _context.Entry(progress).State = EntityState.Modified;
            return Task.CompletedTask; // لا يوجد SaveChanges هنا
        }
    }
}