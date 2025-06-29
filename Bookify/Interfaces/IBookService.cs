using Bookify.DTOs;
using Microsoft.AspNetCore.Http; // For IFormFile
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookify.Interfaces
{
    public interface IBookService
    {
        // --- تعديل ميثود جلب الكتب لتدعم Pagination والفلترة ---
        Task<PaginatedFilteredBooksDto> GetAllBooksAsync(BookFilterDto filter);

        // --- ميثود جلب تفاصيل كتاب ---
        Task<BookDetailDto?> GetBookByIdAsync(int id, string? currentUserId = null);

        // --- ميثود جلب الكتب بناءً على العناوين (للتكامل مع الـ AI) ---
        Task<List<BookListItemDto>> GetBooksByTitlesAsync(List<string> titles);

        // --- ميثود رفع ومعالجة كتاب جديد ---
        Task<BookDetailDto?> UploadAndProcessBookAsync(IFormFile file, string userId);
    }
}