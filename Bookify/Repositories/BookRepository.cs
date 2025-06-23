using Bookify.Contexts;
using Bookify.Entities;
using Bookify.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bookify.Repositories
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
            return await _context.Books.ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetByCategoryAsync(string category)
        {
            // تنظيف وتوحيد حالة الباراميتر القادم لضمان المقارنة الدقيقة
            string normalizedCategoryInput = category.Trim().ToLower();

            return await _context.Books
                               .Where(b => b.Category != null &&
                                           b.Category.Trim().ToLower() == normalizedCategoryInput)
                               .ToListAsync();
        }

        public async Task<Book?> GetByIdAsync(int id)
        {
            return await _context.Books
                                 .Include(b => b.Chapters)
                                 .Include(b => b.Ratings)
                                 .FirstOrDefaultAsync(b => b.BookID == id);
        }

        public async Task<List<Book>> GetByTitlesAsync(List<string> titles)
        {
            if (titles == null || !titles.Any()) return new List<Book>();
            return await _context.Books.Where(b => b.Title != null && titles.Contains(b.Title)).ToListAsync();
        }
    }
}