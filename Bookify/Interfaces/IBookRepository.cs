using Bookify.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookify.Interfaces
{
    public interface IBookRepository
    {
        Task<IEnumerable<Book>> GetAllAsync();
        Task<IEnumerable<Book>> GetByCategoryAsync(string category);
        Task<Book?> GetByIdAsync(int id); // يجب أن يقوم هذا بعمل Include للـ Chapters إذا كان الـ Service سيعتمد عليه
        Task<List<Book>> GetByTitlesAsync(List<string> titles); // <<< تم فك التعليق
        // Task AddAsync(Book book);
        // Task UpdateAsync(Book book);
        // Task DeleteAsync(int id);
    }
}