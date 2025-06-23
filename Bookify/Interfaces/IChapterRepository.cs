using Bookify.Entities;
using System.Threading.Tasks;

namespace Bookify.Interfaces
{
    public interface IChapterRepository
    {
        Task<Chapter?> GetByIdAsync(int chapterId);
        Task<bool> ChapterExistsAndBelongsToBookAsync(int chapterId, int bookId);
        // ممكن نضيف ميثودات تانية هنا لو احتجناها بعدين، زي:
        // Task<IEnumerable<Chapter>> GetChaptersByBookIdAsync(int bookId);
    }
}