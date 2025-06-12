namespace Bookify.DTOs;

public class CreateSpaceDto
{
    public string Title { get; set; }
}

// DTO for representing a participant's information to the client.
// Notice we don't include the full User object, just what's needed.
public class ParticipantDto
{
    public string UserId { get; set; }
    public string UserName { get; set; } // e.g., "John Doe"
    public string Role { get; set; } // "Host", "Speaker", "Listener"
}

// DTO for providing detailed information about a single space.
// This is what a client gets when they view or join a space.
public class SpaceDetailsDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public bool IsActive { get; set; }
    public ParticipantDto Host { get; set; }
    public List<ParticipantDto> Speakers { get; set; } = new List<ParticipantDto>();
    public List<ParticipantDto> Listeners { get; set; } = new List<ParticipantDto>();
    public int TotalParticipants => 1 + Speakers.Count + Listeners.Count; // Calculated property
}

// DTO for listing spaces on a main feed. This is a lightweight version.
public class SpaceSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string HostUserName { get; set; }
    public int ParticipantCount { get; set; }
}

// DTO for the response when a user successfully joins.
public class JoinResponseDto
{
    public string RtcToken { get; set; }
    public string ChannelName { get; set; }
    public uint AgoraUid { get; set; }
}