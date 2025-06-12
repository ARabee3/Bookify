using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // عشان الـ Foreign Key attribute لو حبينا
using System; // عشان DateTime

namespace Bookify.Entities
{
    public class UserBookRating
    {
        // ممكن نعمل Composite Primary Key من UserID و BookID عشان نضمن إن المستخدم يقيم الكتاب مرة واحدة بس
        // أو نخلي RatingID هو الـ PK ونعمل Unique Index على UserID و BookID

        [Key]
        public int RatingID { get; set; } // أو ممكن نحذفه لو هنستخدم Composite Key

        [Required]
        public string UserID { get; set; } // Foreign Key للمستخدم (اللي قيم)
        public virtual ApplicationUser User { get; set; }

        [Required]
        public int BookID { get; set; } // Foreign Key للكتاب (اللي تم تقييمه)
        public virtual Book Book { get; set; }

        [Required]
        [Range(1, 5)] // التقييم من 1 إلى 5 نجوم
        public float Rating { get; set; } // قيمة التقييم (ممكن تبقى int لو مفيش أنصاص نجوم)

        public string? Review { get; set; } // نص المراجعة (اختياري)

        public DateTime RatedAt { get; set; } = DateTime.UtcNow; // تاريخ ووقت التقييم
    }
}