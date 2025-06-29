using Bookify.Entities;
using System.Threading.Tasks;

namespace Bookify.Interfaces
{
    public interface IChapterRepository
    {
        Task<Chapter?> GetByIdAsync(int chapterId);
        Task<bool> ChapterExistsAndBelongsToBookAsync(int chapterId, int bookId);
        Task AddAsync(Chapter chapter); // <<< تم إضافتها
        Task<IEnumerable<Chapter>> GetChaptersByBookIdAsync(int bookId); // <<< تم إضافتها
    }
}