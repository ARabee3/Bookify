using Bookify.Contexts;
using Bookify.DTOs;
using Bookify.Entities;
using Bookify.Hubs;
using Bookify.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SpacesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AgoraService _agoraService;
    private readonly IHubContext<SpaceHub> _hubContext;

    public SpacesController(AppDbContext context, UserManager<ApplicationUser> userManager, AgoraService agoraService, IHubContext<SpaceHub> hubContext)
    {
        _context = context;
        _userManager = userManager;
        _agoraService = agoraService;
        _hubContext = hubContext;
    }

    // --- REFACTORED to use CreateSpaceDto ---
    [HttpPost("create")]
    public async Task<IActionResult> CreateSpace([FromBody] CreateSpaceDto createDto)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var space = new Space { Title = createDto.Title, HostId = user.Id };
        var participant = new Participant { User = user, Space = space, Role = ParticipantRole.Host };

        // This is a simple way to create a unique integer ID for Agora from a GUID
        // You could also use a dedicated integer column in your Participant table.
        participant.AgoraUid = (uint)Math.Abs(user.Id.GetHashCode());

        _context.Spaces.Add(space);
        _context.Participants.Add(participant);
        await _context.SaveChangesAsync();

        // Respond with a more detailed DTO
        var responseDto = new SpaceDetailsDto
        {
            Id = space.Id,
            Title = space.Title,
            IsActive = space.IsActive,
            Host = new ParticipantDto { UserId = user.Id, UserName = user.UserName, Role = "Host" }
        };

        return CreatedAtAction(nameof(GetSpaceById), new { spaceId = space.Id }, responseDto);
    }

    // --- NEW endpoint to get a list of all active spaces ---
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SpaceSummaryDto>>> GetActiveSpaces()
    {
        var spaces = await _context.Spaces
            .Where(s => s.IsActive)
            .Include(s => s.Host) // Include the host user to get their name
            .Include(s => s.Participants) // Include participants to get the count
            .Select(s => new SpaceSummaryDto // Project directly into the DTO
            {
                Id = s.Id,
                Title = s.Title,
                HostUserName = s.Host.UserName,
                ParticipantCount = s.Participants.Count
            })
            .ToListAsync();

        return Ok(spaces);
    }

    // --- NEW endpoint to get details of a single space ---
    [HttpGet("{spaceId}")]
    public async Task<ActionResult<SpaceDetailsDto>> GetSpaceById(Guid spaceId)
    {
        var space = await _context.Spaces
            .Include(s => s.Participants)
                .ThenInclude(p => p.User) // We need the user for their name
            .FirstOrDefaultAsync(s => s.Id == spaceId);

        if (space == null || !space.IsActive) return NotFound();

        var host = space.Participants.First(p => p.Role == ParticipantRole.Host);

        var responseDto = new SpaceDetailsDto
        {
            Id = space.Id,
            Title = space.Title,
            IsActive = space.IsActive,
            Host = new ParticipantDto { UserId = host.UserId, UserName = host.User.UserName, Role = "Host" },
            Speakers = space.Participants
                .Where(p => p.Role == ParticipantRole.Speaker)
                .Select(p => new ParticipantDto { UserId = p.UserId, UserName = p.User.UserName, Role = "Speaker" })
                .ToList(),
            Listeners = space.Participants
                .Where(p => p.Role == ParticipantRole.Listener)
                .Select(p => new ParticipantDto { UserId = p.UserId, UserName = p.User.UserName, Role = "Listener" })
                .ToList()
        };

        return Ok(responseDto);
    }

    // --- REFACTORED to return a specific JoinResponseDto ---
    [HttpPost("{spaceId}/join")]
    public async Task<ActionResult<JoinResponseDto>> JoinSpace(Guid spaceId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var space = await _context.Spaces.FindAsync(spaceId);
        if (space == null || !space.IsActive) return NotFound("Space not found.");

        var participant = await _context.Participants
            .FirstOrDefaultAsync(p => p.SpaceId == spaceId && p.UserId == user.Id);

        if (participant == null)
        {
            participant = new Participant
            {
                UserId = user.Id,
                SpaceId = spaceId,
                Role = ParticipantRole.Listener,
                AgoraUid = (uint)Math.Abs(user.Id.GetHashCode()) // Assign Agora UID on join
            };
            _context.Participants.Add(participant);
            await _context.SaveChangesAsync();

            // Notify via SignalR that a new listener has joined
            await _hubContext.Clients.Group(spaceId.ToString()).SendAsync("UserJoined", new ParticipantDto { UserId = user.Id, UserName = user.UserName, Role = "Listener" });
        }

        string token = _agoraService.GenerateRtcToken(spaceId.ToString(), participant.AgoraUid);

        return Ok(new JoinResponseDto
        {
            RtcToken = token,
            ChannelName = spaceId.ToString(),
            AgoraUid = participant.AgoraUid
        });
    }
}