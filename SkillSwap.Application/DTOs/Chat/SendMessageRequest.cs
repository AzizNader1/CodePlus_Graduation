using SkillSwap.Domain.Enums;


namespace SkillSwap.Application.DTOs;

public class SendMessageRequest
{
    public string Content { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
}
