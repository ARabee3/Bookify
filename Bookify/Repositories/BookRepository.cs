using Bookify.Contexts;
using Bookify.DTOs; // <<< لإضافة BookFilterDto
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

        public async Task<(IEnumerable<Book> books, int totalCount)> GetAllFilteredAndPaginatedAsync(BookFilterDto filter)
        {
            var query = _context.Books.AsQueryable();

            // تطبيق الفلترة
            if (!string.IsNullOrEmpty(filter.Category))
            {
                query = query.Where(b => b.Category != null && b.Category.ToLower() == filter.Category.ToLower());
            }
            // يمكن إضافة فلاتر أخرى هنا

            // حساب العدد الكلي (قبل الـ Pagination)
            var totalCount = await query.CountAsync();

            // تطبيق الـ Pagination
            var books = await query
                              .Skip((filter.PageNumber - 1) * filter.PageSize)
                              .Take(filter.PageSize)
                              .ToListAsync();

            return (books, totalCount);
        }

        public async Task<Book?> GetByIdWithDetailsAsync(int id) // <<< تم تغيير الاسم
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

        public async Task AddAsync(Book book) // <<< تم إضافتها
        {
            await _context.Books.AddAsync(book);
        }
    }
}