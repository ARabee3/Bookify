using Bookify.DTOs; // <<< لإضافة BookFilterDto
using Bookify.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookify.Interfaces
{
    public interface IBookRepository
    {
        // --- تم تعديل هذه الميثود لتأخذ DTO ---
        Task<(IEnumerable<Book> books, int totalCount)> GetAllFilteredAndPaginatedAsync(BookFilterDto filter);

        Task<Book?> GetByIdWithDetailsAsync(int id); // <<< تم تغيير الاسم
        Task<List<Book>> GetByTitlesAsync(List<string> titles);
        Task AddAsync(Book book); // <<< تم إضافتها
        // ... (باقي الميثودات مثل Update, Delete لو احتجناهم) ...
    }
}