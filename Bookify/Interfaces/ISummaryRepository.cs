using Bookify.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookify.Interfaces // أو اسم الـ Namespace بتاعك
{
    public interface ISummaryRepository
    {
        // لجلب كل ملخصات الشابترات لكتاب معين
        Task<IEnumerable<Summary>> GetSummariesByBookIdAsync(int bookId);

        // لجلب ملخص شابتر معين بالـ ID بتاعه
        Task<Summary?> GetSummaryByChapterIdAsync(int chapterId);

        // لإضافة ملخص جديد (هنستخدمها في الـ Service بعدين)
        Task AddAsync(Summary summary);

        // ممكن نضيف Update/Delete لو احتجناهم بعدين
        // Task UpdateAsync(Summary summary);
        // Task DeleteAsync(int summaryId);
    }
}