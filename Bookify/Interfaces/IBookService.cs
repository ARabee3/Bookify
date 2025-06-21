using Bookify.DTOs; // <<< تأكد إنها موجودة
using Bookify.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookify.Interfaces
{
    public interface IBookService
    {
        // --- تم تعديل الـ Return Type هنا ---
        Task<IEnumerable<BookListItemDto>> GetAllBooksAsync(string? category);
        // ------------------------------------

        Task<BookDetailDto?> GetBookByIdAsync(int id, string? currentUserId = null);

        // دي ممكن نرجع منها List<BookListItemDto> لو حابين
        Task<List<BookListItemDto>> GetBooksByTitlesAsync(List<string> titles);
        // أو لو الـ AI API بترجع بيانات كافية، ممكن نستخدمها مباشرة
        // Task<List<AiBookDto>> GetAiBooksByTitlesAsync(List<string> titles);
    }
}