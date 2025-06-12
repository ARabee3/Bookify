using Bookify.Entities; // أو DTOs
using Bookify.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookify.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository; // نعتمد على الـ Interface

        public BookService(IBookRepository bookRepository) // نعمل Inject للـ Interface
        {
            _bookRepository = bookRepository;
        }

        public async Task<IEnumerable<Book>> GetAllBooksAsync(string? category)
        {
            // هنا الـ Business Logic (لو فيه)
            // حالياً، هنفلتر هنا
            if (!string.IsNullOrEmpty(category))
            {
                return await _bookRepository.GetByCategoryAsync(category);
            }
            else
            {
                return await _bookRepository.GetAllAsync();
            }
        }

        public async Task<Book?> GetBookByIdAsync(int id)
        {
            // ممكن نضيف Logic هنا، مثلاً التحقق من صلاحية المستخدم قبل استدعاء الـ Repo
            return await _bookRepository.GetByIdAsync(id);
        }

        //public async Task<List<Book>> GetBooksByTitlesAsync(List<string> titles)
        //{
        //    return await _bookRepository.GetByTitlesAsync(titles);
        //}
    }
}