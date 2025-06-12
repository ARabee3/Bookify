using Bookify.Entities; // أو DTOs لو هنرجع DTOs
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookify.Interfaces
{
    public interface ISummaryService
    {
        Task<IEnumerable<Summary>> GetChapterSummariesForBookAsync(int bookId);
        Task<Summary?> GetSummaryForChapterAsync(int chapterId);

        // ميثود لإضافة ملخص عام للكتاب (هتحتاج الـ UserID)
        Task<Summary> AddBookSummaryAsync(int bookId, string content, string userId);

        // ممكن نضيف ميثودات تانية للـ Business Logic هنا لو فيه
    }
}