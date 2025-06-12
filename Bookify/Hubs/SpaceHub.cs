namespace Bookify.Hubs;

using Microsoft.AspNetCore.SignalR;

public class SpaceHub : Hub
{
    // A client calls this to join the SignalR group for a specific space
    public async Task JoinSpaceGroup(string spaceId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, spaceId);
    }

    // A client calls this to leave the group
    public async Task LeaveSpaceGroup(string spaceId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, spaceId);
    }

    // Example: A client can send a message
    public async Task SendMessage(string spaceId, string user, string message)
    {
        // This sends the message to all clients in the same space group
        await Clients.Group(spaceId).SendAsync("ReceiveMessage", user, message);
    }
}