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
            // لا نحتاج Include(b => b.Ratings) هنا لأن BookListItemDto يستخدم Book.Rating مباشرة
            return await _context.Books.ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetByCategoryAsync(string category)
        {
            string trimmedCategory = category.Trim().ToLower(); // جهز الباراميتر بره
            return await _context.Books
                               .Where(b => b.Category != null && b.Category.Trim().ToLower() == trimmedCategory)
                               .ToListAsync();
        }

        public async Task<Book?> GetByIdAsync(int id)
        {
            // --- تم التأكيد على الـ Includes المطلوبة لـ BookDetailDto ---
            return await _context.Books
                                 .Include(b => b.Chapters) // لجلب Chapters Collection
                                 .Include(b => b.Ratings)  // لجلب Ratings Collection (UserBookRating)
                                 .FirstOrDefaultAsync(b => b.BookID == id);
        }

        public async Task<List<Book>> GetByTitlesAsync(List<string> titles)
        {
            if (titles == null || !titles.Any()) return new List<Book>();
            // لا نحتاج Include(b => b.Ratings) هنا لأن BookListItemDto يستخدم Book.Rating مباشرة
            return await _context.Books.Where(b => b.Title != null && titles.Contains(b.Title)).ToListAsync();
        }
    }
}