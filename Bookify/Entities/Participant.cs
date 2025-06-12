namespace Bookify.Entities;
public class Participant
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public Guid SpaceId { get; set; }
    public ParticipantRole Role { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public uint AgoraUid { get; set; } // Add this line

    // Navigation properties
    public ApplicationUser User { get; set; }
    public Space Space { get; set; }
}
