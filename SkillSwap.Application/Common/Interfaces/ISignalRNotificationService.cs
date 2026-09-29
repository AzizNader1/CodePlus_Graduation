namespace SkillSwap.Application.Common.Interfaces;

public interface ISignalRNotificationService
{
    Task SendMessageToUserAsync(Guid userId, string method, object data);
    Task BroadcastToConversationAsync(Guid conversationId, string method, object data);
}
