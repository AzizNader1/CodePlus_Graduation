using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SkillSwap.Infrastructure.Hubs;

[Authorize]
public class ChatHub : Hub
{
    public async Task JoinConversation(string conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"conv_{conversationId}");
    }

    public async Task LeaveConversation(string conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conv_{conversationId}");
    }

    public async Task SendTyping(string conversationId, bool isTyping)
    {
        var userName = Context.User?.FindFirst("name")?.Value ?? "A user";
        await Clients.OthersInGroup($"conv_{conversationId}").SendAsync("UserTyping", new
        {
            ConversationId = conversationId,
            UserName = userName,
            IsTyping = isTyping
        });
    }
}
