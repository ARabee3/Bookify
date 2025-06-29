using System.ComponentModel.DataAnnotations;
namespace Bookify.Entities
{
    public class Chapter
    {
        [Key]
        public int ChapterID { get; set; }
        public string Title { get; set; }
        public int ChapterNumber { get; set; }

        public int? StartPage { get; set; }
        public int? EndPage { get; set; }
        public int BookID { get; set; }
        public Book Book { get; set; }
        public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
    
}
}
