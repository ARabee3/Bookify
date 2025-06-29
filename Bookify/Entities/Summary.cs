using System;
using System.ComponentModel.DataAnnotations;

namespace Bookify.Entities
{
    public class Summary
    {
        [Key]
        public int SummaryID { get; set; }

        public int? BookID { get; set; }
        public virtual Book? Book { get; set; }

        public int? ChapterID { get; set; }
        public virtual Chapter? Chapter { get; set; }

        public string? UserID { get; set; }
        public virtual ApplicationUser? User { get; set; }

        public string Content { get; set; }
        public string? Source { get; set; }

        // --- تم تعديل هذه لتكون غير Nullable ---
        public DateTime CreateDate { get; set; }
        // ----------------------------------------
    }
}