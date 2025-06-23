using System.ComponentModel.DataAnnotations;
namespace Bookify.DTOs
{
    public class CreateNoteDto
    {
        public int? BookID { get; set; } // يبعت ده أو اللي بعده
        public int? ChapterID { get; set; }

        [Required]
        [StringLength(2000, MinimumLength = 1)]
        public string Content { get; set; }
    }
}