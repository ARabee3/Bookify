namespace Bookify.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Space
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public string HostId { get; set; }
    [ForeignKey("HostId")]
    public ApplicationUser Host { get; set; } // Link to the host user
    public ICollection<Participant> Participants { get; set; } = new List<Participant>();
}