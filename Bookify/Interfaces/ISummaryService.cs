using Bookify.DTOs; // <<< تم إضافة using
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookify.Interfaces
{
    public interface ISummaryService
    {
        // ميثودات جلب الملخصات الموجودة
        Task<IEnumerable<SummaryDto>> GetSummariesForBookAsync(int bookId); // <<< تم تغيير الاسم ليكون أوضح
        Task<SummaryDto?> GetSummaryForChapterAsync(int chapterId);
        Task<SummaryDto?> GetOrCreateSummaryForChapterAsync(int chapterId);


        // ميثود لتوليد وحفظ ملخص جديد
        Task<SummaryDto?> GenerateAndSaveSummaryForChapterAsync(int chapterId);
    }
}