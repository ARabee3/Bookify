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

        public async Task<IEnumerable<Book>> GetAllAsync(int pageNumber, int pageSize)
        {
            return await _context.Books
                                 .OrderBy(b => b.BookID) 
                                 .Skip((pageNumber - 1) * pageSize)
                                 .Take(pageSize)
                                 .ToListAsync();

            
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Books.CountAsync();
        }


        public async Task<IEnumerable<Book>> GetByCategoryAsync(string category)
        {
            // تنظيف وتوحيد حالة الباراميتر القادم لضمان المقارنة الدقيقة
            string normalizedCategoryInput = category.Trim().ToLower();


            return await _context.Books
                               .Where(b => b.Category != null &&
                                           b.Category.ToLower().Contains(normalizedCategoryInput))
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

            var lowerCaseTitles = titles.Select(t => t.ToLower()).ToList();

            return await _context.Books
                .Where(b => b.Title != null && lowerCaseTitles.Contains(b.Title.ToLower()))
                .ToListAsync();
        }
    }
}