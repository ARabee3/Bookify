using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // عشان Foreign Key attribute لو حابين

namespace Bookify.Entities
{
    public class UserDailyActivityLog
    {
        [Key]
        public int ActivityLogID { get; set; } // Primary Key

        [Required]
        public string UserID { get; set; } // Foreign Key للمستخدم
        public virtual ApplicationUser User { get; set; }

        // هنخزن التاريخ فقط (بدون وقت) عشان يبقى سهل المقارنة بين الأيام
        // وهنستخدم نوع Date في SQL Server لو متاح (EF Core بيعرف يتعامل مع ده)
        [Required]
        [Column(TypeName = "date")] // بيحدد نوع العمود في SQL Server كـ 'date'
        public DateTime ActivityDate { get; set; }

        // اختياري: ممكن نضيف وقت تسجيل النشاط بالظبط لو حبينا
        // public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
    }
}