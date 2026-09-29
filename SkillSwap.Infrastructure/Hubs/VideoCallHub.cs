using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SkillSwap.Infrastructure.Hubs;

[Authorize]
public class VideoCallHub : Hub
{
    public async Task JoinCall(Guid sessionId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? Context.User?.FindFirst("sub")?.Value;
        var userName = Context.User?.FindFirst("name")?.Value ?? Context.User?.Identity?.Name ?? "Participant";

        await Groups.AddToGroupAsync(Context.ConnectionId, $"call_{sessionId}");

        await Clients.OthersInGroup($"call_{sessionId}").SendAsync("PeerJoined", new
        {
            SessionId = sessionId,
            UserId = userId,
            UserName = userName
        });
    }

    public async Task SendOffer(Guid sessionId, string sdpOffer)
    {
        await Clients.OthersInGroup($"call_{sessionId}").SendAsync("ReceiveOffer", sdpOffer);
    }

    public async Task SendAnswer(Guid sessionId, string sdpAnswer)
    {
        await Clients.OthersInGroup($"call_{sessionId}").SendAsync("ReceiveAnswer", sdpAnswer);
    }

    public async Task SendIceCandidate(Guid sessionId, object candidate)
    {
        await Clients.OthersInGroup($"call_{sessionId}").SendAsync("ReceiveIceCandidate", candidate);
    }

    public async Task SendMediaState(Guid sessionId, bool audioMuted, bool videoMuted, bool isScreenSharing)
    {
        await Clients.OthersInGroup($"call_{sessionId}").SendAsync("PeerMediaStateChanged", new
        {
            AudioMuted = audioMuted,
            VideoMuted = videoMuted,
            IsScreenSharing = isScreenSharing
        });
    }

    public async Task EndCall(Guid sessionId)
    {
        var userName = Context.User?.FindFirst("name")?.Value ?? Context.User?.Identity?.Name ?? "Participant";
        await Clients.OthersInGroup($"call_{sessionId}").SendAsync("CallEnded", $"{userName} has ended the call.");
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"call_{sessionId}");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
