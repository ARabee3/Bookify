using System.ComponentModel.DataAnnotations;

namespace Bookify.DTOs
{
    public class UpdateProgressDto
    {
        [Required(ErrorMessage = "Book ID is required.")]
        public int BookID { get; set; }

        // public int? LastReadChapterID { get; set; } // <<< تم الحذف

        public int? LastReadPageNumber { get; set; } // رقم آخر صفحة قرأها المستخدم

        [Range(0, 100, ErrorMessage = "Completion percentage must be between 0 and 100.")]
        public float? CompletionPercentage { get; set; } // اختياري، سيتم حسابه إذا لم يرسل
    }
}