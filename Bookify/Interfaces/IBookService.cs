using Bookify.Entities; // أو DTOs لو هنستخدمها هنا
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookify.Interfaces
{
    public interface IBookService
    {
        Task<IEnumerable<Book>> GetAllBooksAsync(string? category); // الميثود دي بتاخد الكاتيجوري كباراميتر
        Task<Book?> GetBookByIdAsync(int id);

        //Task<List<Book>> GetBooksByTitlesAsync(List<string> titles);
        // ممكن نضيف ميثودات للـ Business Logic هنا بعدين

    }
}