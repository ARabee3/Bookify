using Bookify.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookify.Interfaces
{
    public interface ISummaryRepository
    {
        Task AddAsync(Summary summary);
        // Task<Summary?> GetByIdAsync(int summaryId); // ممكن نضيفها لو احتجناها
        Task<IEnumerable<Summary>> GetSummariesForBookAsync(int bookId); // <<< تم إضافتها
        Task<Summary?> GetSummaryForChapterAsync(int chapterId); // <<< تم إضافتها
        // Update و Delete ممكن نضيفهم لو احتجناهم
    }
}