using System;
using System.Collections.Generic;

namespace Bookify.DTOs
{
    

    public class BookDetailDto
    {
        public int BookID { get; set; }
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? Category { get; set; }
        public string? Source { get; set; }

        public string? PdfFileUrl { get; set; }
        public string? CoverImageUrl { get; set; }

        public string? Description { get; set; }
        public string? Difficulty { get; set; }
        public float? AverageRating { get; set; }
        public int? TotalRatings { get; set; }
        
        
  
        public int? TotalPages { get; set; }




        public int? Views { get; set; }
        public string? Language { get; set; }
        public int? ReleaseYear { get; set; }
        public string? Prerequisites { get; set; }
        public string? LearningObjectives { get; set; }





        // --- تمت إضافة هذه الخاصية ---
        public int? TotalChapters { get; set; } // عدد الشابترات الكلي للكتاب
        // ---------------------------

        public List<ChapterSummaryDto>? Chapters { get; set; }

        // ... (باقي الخصائص الاختيارية المعلقة) ...
    }
}