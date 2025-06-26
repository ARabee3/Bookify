using Bookify.DTOs; // عشان BookListItemDto
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookify.Interfaces
{
    public interface IUserLibraryService
    {
        Task<bool> AddBookToMyLibraryAsync(string userId, int bookId);
        Task<bool> RemoveBookFromMyLibraryAsync(string userId, int bookId);
        Task<IEnumerable<BookListItemDto>> GetMyLibraryBooksAsync(string userId, int pageNumber, int pageSize); // مع Pagination
        Task<bool> CheckIfBookInMyLibraryAsync(string userId, int bookId);
    }
}