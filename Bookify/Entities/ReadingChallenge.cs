using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookify.Entities
{
    public class ReadingChallenge
    {
        [Key]
        public int ChallengeID { get; set; } // Primary Key

        [Required]
        public string UserID { get; set; } // Foreign Key للمستخدم
        public virtual ApplicationUser User { get; set; }

        [Required]
        public int Year { get; set; } // سنة التحدي، مثال: 2024, 2025

        [Required]
        [Range(1, int.MaxValue)] // الهدف لازم يكون على الأقل كتاب واحد
        public int TargetBooksCount { get; set; } // عدد الكتب المستهدف قراءتها

        [Required]
        [Range(0, int.MaxValue)] // عدد الكتب المكتملة يبدأ من صفر
        public int BooksCompletedCount { get; set; } = 0; // عدد الكتب التي أكملها المستخدم فعلاً

        // ممكن نضيف تواريخ بداية ونهاية تلقائية أو نتركها للـ Logic
        // public DateTime StartDate { get; set; }
        // public DateTime EndDate { get; set; }

        // public bool IsActive { get; set; } = true; // هل هذا هو التحدي النشط للسنة؟
    }
}