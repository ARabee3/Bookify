using Bookify.Contexts;
using Bookify.Entities;
using Bookify.Interfaces; // عشان الـ Interface
using Microsoft.EntityFrameworkCore; // عشان الـ EF Core methods
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bookify.Repositories // أو اسم الـ Namespace بتاعك
{
    public class BookRepository : IBookRepository
    {
        private readonly AppDbContext _context;

        public BookRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Book>> GetAllAsync()
        {
            // نفس كود جلب كل الكتب (بدون فلترة هنا)
            return await _context.Books.ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetByCategoryAsync(string category)
        {
            // نفس كود الفلترة
            return await _context.Books
                               .Where(b => b.Category != null && b.Category.ToLower() == category.ToLower())
                               .ToListAsync();
        }

        public async Task<Book?> GetByIdAsync(int id)
        {
            // نفس كود جلب الكتاب بالـ ID
            return await _context.Books.FindAsync(id);
        }

        // Implement Add, Update, Delete later if needed

        //public async Task<List<Book>> GetByTitlesAsync(List<string> titles)
        //{
        //    if (titles == null || !titles.Any()) return new List<Book>();
        //    return await _context.Books.Where(b => b.Title != null && titles.Contains(b.Title)).ToListAsync();
        //}

    }
}