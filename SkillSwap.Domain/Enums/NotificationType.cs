namespace SkillSwap.Domain.Enums;

/// <summary>
/// Notification categories for in-app and push delivery.
/// </summary>
public enum NotificationType
{
    SwapRequestReceived = 1,
    SwapRequestAccepted = 2,
    SwapRequestRejected = 3,
    SwapRequestCountered = 4,
    SessionScheduled = 5,
    SessionReminder = 6,
    SessionCompleted = 7,
    ChatMessageReceived = 8,
    ReviewReceived = 9,
    SystemAlert = 10
}
