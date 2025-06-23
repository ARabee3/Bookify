using System.ComponentModel.DataAnnotations;
namespace Bookify.DTOs
{
    public class UpdateNoteDto
    {
        [Required]
        [StringLength(2000, MinimumLength = 1)]
        public string Content { get; set; }
    }
}