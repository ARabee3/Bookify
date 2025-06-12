using System.ComponentModel.DataAnnotations;
namespace Bookify.Entities
{
    public enum RecommendationLevel { Basic, Medium, Advanced }
    public class Recommendation
    {
        [Key]
        public int RecommendationID { get; set; }
        public string UserID { get; set; }
        public int BookID { get; set; }
        public RecommendationLevel Level { get; set; } = RecommendationLevel.Medium;

        public ApplicationUser User { get; set; }
        public Book Book { get; set; }
    }
}
