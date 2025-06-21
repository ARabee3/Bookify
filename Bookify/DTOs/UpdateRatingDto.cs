// في مجلد Dtos
using System.ComponentModel.DataAnnotations;

namespace Bookify.DTOs
{
    public class UpdateRatingDto
    {
        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public float RatingValue { get; set; }

        public string? ReviewText { get; set; }
    }
}