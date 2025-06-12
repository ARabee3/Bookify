using System.ComponentModel.DataAnnotations;

namespace Bookify.DTOs
{
    public class UpdateProgressDto
    {
        [Required]
        public int BookID { get; set; }

        public int? LastReadChapterID { get; set; } // ID الشابتر اللي وصله

        [Range(0, 100)] // النسبة بين 0 و 100
        public float? CompletionPercentage { get; set; } // ممكن الـ Frontend يبعتها أو الـ Backend يحسبها

        public int? LastReadPageNumber { get; set; } // لو هنستخدمها

        // الـ Status ممكن الـ Backend يحددها
    }
}