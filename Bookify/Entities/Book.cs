using System;
using System.ComponentModel.DataAnnotations;
namespace Bookify.Entities
{
    public class Book
    {

        public string? PdfFilePath { get; set; } 
        public int BookID { get; set; }
        public string Title { get; set; }
        public string? Author { get; set; }
        public string? Category { get; set; }
        public string Source { get; set; }
        public string? UploadedBy { get; set; }

        // في Book.cs
        public int? TotalPages { get; set; } // إجمالي عدد صفحات الكتاب (Nullable)

        public ApplicationUser Uploader { get; set; }
        public ICollection<Recommendation> Recommendations { get; set; } = new List<Recommendation>();
        public ICollection<Progress> Progresses { get; set; } = new List<Progress>();
        public ICollection<Summary> Summaries { get; set; } = new List<Summary>();
        public ICollection<UserBookRating> Ratings { get; set; } = new List<UserBookRating>();
        public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
        public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();


    }
}
