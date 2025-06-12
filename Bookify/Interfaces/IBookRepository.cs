using Bookify.Entities; // عشان نستخدم Book
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookify.Interfaces // أو اسم الـ Namespace بتاعك لو مختلف
{
    public interface IBookRepository
    {
        Task<IEnumerable<Book>> GetAllAsync(); // تجيب كل الكتب
        Task<IEnumerable<Book>> GetByCategoryAsync(string category); // تجيب الكتب حسب الكاتيجوري
        Task<Book?> GetByIdAsync(int id); // تجيب كتاب بالـ ID (ممكن يرجع null)
        // لسه مش محتاجين Add, Update, Delete دلوقتي بس ممكن نضيفهم كـ Structure
        // Task AddAsync(Book book);
        // Task UpdateAsync(Book book);
        // Task DeleteAsync(int id);

        //Task<List<Book>> GetByTitlesAsync(List<string> titles);
    }
}