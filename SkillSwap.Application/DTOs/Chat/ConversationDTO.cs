using SkillSwap.Domain.Enums;


namespace SkillSwap.Application.DTOs;

public class ConversationDTO
{
    public Guid Id { get; set; }
    public Guid PartnerId { get; set; }
    public string PartnerName { get; set; } = string.Empty;
    public string? PartnerAvatarUrl { get; set; }
    public string? LastMessageContent { get; set; }
    public DateTime LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
}
