using Bookify.Contexts;
using Bookify.Entities;
using Bookify.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bookify.Repositories
{
    public class UserLibraryRepository : IUserLibraryRepository
    {
        private readonly AppDbContext _context;

        public UserLibraryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddBookAsync(UserLibraryBook userLibraryBook)
        {
            await _context.UserLibraryBooks.AddAsync(userLibraryBook);
        }

        public async Task RemoveBookAsync(UserLibraryBook userLibraryBook)
        {
            _context.UserLibraryBooks.Remove(userLibraryBook);
            await Task.CompletedTask;
        }

        public async Task<UserLibraryBook?> GetUserLibraryBookAsync(string userId, int bookId)
        {
            return await _context.UserLibraryBooks
                                 .FirstOrDefaultAsync(ulb => ulb.UserID == userId && ulb.BookID == bookId);
        }

        // --- تم تعديل هذه الميثود ---
        public async Task<IEnumerable<Book>> GetUserLibraryBooksAsync(string userId)
        {
            // نجلب الكتب نفسها التي أضافها المستخدم لمكتبته مع تضمين البيانات اللازمة للـ DTO
            return await _context.UserLibraryBooks
                                 .Where(ulb => ulb.UserID == userId)
                                 .OrderByDescending(ulb => ulb.AddedAt)
                                 .Include(ulb => ulb.Book) // <<< أولاً، نعمل Include للـ Book Entity نفسها
                                                           // .ThenInclude(b => b.Ratings) // <<< ثم، لو كنا سنحسب AverageRating من UserBookRating هنا
                                                           // .ThenInclude(b => b.Chapters) // <<< لو كنا سنحتاج تفاصيل الشابترات هنا
                                 .Select(ulb => ulb.Book) // <<< بعد ذلك، نختار الـ Book Entity
                                 .Where(b => b != null) // للتأكد أن الكتاب ليس null (احتياطي)
                                 .ToListAsync();
        }
        // --- نهاية التعديل ---

        public async Task<bool> IsBookInUserLibraryAsync(string userId, int bookId)
        {
            return await _context.UserLibraryBooks.AnyAsync(ulb => ulb.UserID == userId && ulb.BookID == bookId);
        }
    }
}