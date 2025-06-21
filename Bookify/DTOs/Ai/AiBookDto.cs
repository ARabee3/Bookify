using System.Text.Json.Serialization;
namespace Bookify.DTOs.Ai
{
    public class AiBookDto
    {
        [JsonPropertyName("book_id")]
        public int BookId { get; set; }
        [JsonPropertyName("title")]
        public string? Title { get; set; }
        [JsonPropertyName("genre")]
        public string? Genre { get; set; }
        [JsonPropertyName("summary")]
        public string? Summary { get; set; }
        [JsonPropertyName("difficulty")]
        public string? Difficulty { get; set; }
        [JsonPropertyName("length_pages")]
        public int? LengthPages { get; set; }
        [JsonPropertyName("rating")]
        public float? Rating { get; set; }
        [JsonPropertyName("views")]
        public int? Views { get; set; }
        [JsonPropertyName("language")]
        public string? Language { get; set; }
        [JsonPropertyName("release_year")]
        public int? ReleaseYear { get; set; }
        [JsonPropertyName("author")]
        public string? Author { get; set; }
        [JsonPropertyName("prerequisites")]
        public string? Prerequisites { get; set; }
        [JsonPropertyName("learning_objectives")]
        public string? LearningObjectives { get; set; }
        [JsonPropertyName("combined_score")]
        public float? CombinedScore { get; set; }
    }
}