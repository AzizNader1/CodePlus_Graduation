using Microsoft.AspNetCore.SignalR;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Infrastructure.Hubs;

namespace SkillSwap.Infrastructure.Services;

public class SignalRNotificationService : ISignalRNotificationService
{
    private readonly IHubContext<NotificationHub> _notificationHub;
    private readonly IHubContext<ChatHub> _chatHub;

    public SignalRNotificationService(
        IHubContext<NotificationHub> notificationHub,
        IHubContext<ChatHub> chatHub)
    {
        _notificationHub = notificationHub;
        _chatHub = chatHub;
    }

    public async Task SendMessageToUserAsync(Guid userId, string method, object data)
    {
        await _notificationHub.Clients.Group($"user_{userId}").SendAsync(method, data);
    }

    public async Task BroadcastToConversationAsync(Guid conversationId, string method, object data)
    {
        await _chatHub.Clients.Group($"conv_{conversationId}").SendAsync(method, data);
    }
}
