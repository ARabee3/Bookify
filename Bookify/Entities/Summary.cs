using System.ComponentModel.DataAnnotations;
namespace Bookify.Entities
{
    public class Summary
    {
        [Key]
        public int SummaryID { get; set; }
        public string? UserID { get; set; }
        public int? BookID { get; set; }
        public DateTime? CreateDate { get; set; } = DateTime.Now;
        public string Content { get; set; }
        public string? Source { get; set; }

        public ApplicationUser? User { get; set; }

        public int? ChapterID { get; set; }

        public virtual Chapter? Chapter { get; set; } // Navigation property للشابتر


        public Book? Book { get; set; }
    }
}
