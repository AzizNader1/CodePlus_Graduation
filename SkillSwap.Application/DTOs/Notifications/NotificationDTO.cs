using SkillSwap.Domain.Enums;


namespace SkillSwap.Application.DTOs;

public class NotificationDTO
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public string? TargetUrl { get; set; }
    public Guid? TargetReferenceId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
