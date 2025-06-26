using Bookify.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookify.Interfaces
{
    public interface IUserLibraryRepository
    {
        Task AddBookAsync(UserLibraryBook userLibraryBook);
        Task RemoveBookAsync(UserLibraryBook userLibraryBook); // نمرر الـ Entity للحذف
        Task<UserLibraryBook?> GetUserLibraryBookAsync(string userId, int bookId); // للتحقق أو الحذف
        Task<IEnumerable<Book>> GetUserLibraryBooksAsync(string userId); // ترجع كتب المستخدم
        Task<bool> IsBookInUserLibraryAsync(string userId, int bookId);
    }
}