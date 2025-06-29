using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Bookify.Entities
{
    public class Book
    {
        [Key] // أو اتركه للـ Convention
        public int BookID { get; set; }

        [Required]
        public string Title { get; set; }
        public string? Author { get; set; }
        public string? Category { get; set; } // Genre

        [Required]
        public string Source { get; set; }

        public string? UploadedBy { get; set; } // FK to ApplicationUser
        public virtual ApplicationUser? Uploader { get; set; } // تم تعديلها لـ Nullable

        public string? PdfFilePath { get; set; }
        public string? CoverImagePath { get; set; }
        public int? TotalPages { get; set; }

        [Required]
        public bool IsPublic { get; set; } = false; // Default to private

        public int? Views { get; set; }
        public string? Language { get; set; }
        public int? ReleaseYear { get; set; }
        public string? Prerequisites { get; set; }
        public string? LearningObjectives { get; set; }




        // --- الخصائص الإضافية المطلوبة للـ DTO والـ Recommendation ---
        public string? Summary { get; set; } // ملخص قصير للكتاب
        public string? Difficulty { get; set; } // Beginner, Intermediate, Advanced
        public float? Rating { get; set; } // متوسط التقييم العام (يمكن حسابه أو تخزينه)
       
        
       
        // --- نهاية الخصائص الإضافية ---

        // Navigation Properties
        public virtual ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
        public virtual ICollection<Summary> Summaries { get; set; } = new List<Summary>(); // ملخصات عامة للكتاب
        public virtual ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
        public virtual ICollection<UserBookRating> Ratings { get; set; } = new List<UserBookRating>(); // <<< اسم الـ Collection هنا
        public virtual ICollection<Progress> Progresses { get; set; } = new List<Progress>();
        public virtual ICollection<Recommendation> Recommendations { get; set; } = new List<Recommendation>();

   
    }
}