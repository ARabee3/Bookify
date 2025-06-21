using Bookify.Entities;

public class UserBookRating
{
    public int RatingID { get; set; } // PK, Identity
    public string UserID { get; set; } // FK to AspNetUsers, Required, OnDelete: Restrict
    public int BookID { get; set; } // FK to Books, Required, OnDelete: Cascade
    public float Rating { get; set; } // Required, Range(1,5) المفروض نحطها هنا
    public string? Review { get; set; } // Nullable
    public DateTime RatedAt { get; set; } = DateTime.UtcNow; // المفروض ليها Default value أو تتظبط في الـ Service

    public virtual ApplicationUser User { get; set; }
    public virtual Book Book { get; set; }
}