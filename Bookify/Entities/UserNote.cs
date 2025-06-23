using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookify.Entities
{
    public class UserNote
    {
        [Key]
        public int NoteID { get; set; } // مفتاح أساسي للنوت

        [Required]
        public string UserID { get; set; } // مفتاح أجنبي للمستخدم صاحب النوت
        public virtual ApplicationUser User { get; set; }

        // النوت ممكن تكون مرتبطة بكتاب بشكل عام، أو بشابتر معين، أو حتى بصفحة معينة لو حبينا
        // هنخليها حالياً مرتبطة إما بكتاب أو بشابتر

        public int? BookID { get; set; } // مفتاح أجنبي للكتاب (اختياري)
        public virtual Book? Book { get; set; }

        public int? ChapterID { get; set; } // مفتاح أجنبي للشابتر (اختياري)
        public virtual Chapter? Chapter { get; set; }

        // (متقدم) لو عايزين نربط النوت بصفحة معينة في الـ PDF
        // public int? PageNumberInPdf { get; set; }
        // public string? HighlightedText { get; set; } // النص اللي المستخدم عمله Highlight لو النوت مرتبطة بتحديد

        [Required]
        [StringLength(2000, MinimumLength = 1)] // حد أقصى وأدنى لطول النوت
        public string Content { get; set; } // محتوى النوت نفسه

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastModifiedAt { get; set; } = DateTime.UtcNow;

        // (اختياري) ممكن نضيف Tags أو لون للنوت
        // public string? Tags { get; set; } // ممكن تكون comma-separated
        // public string? NoteColor { get; set; } // مثلاً "yellow", "blue"
    }
}