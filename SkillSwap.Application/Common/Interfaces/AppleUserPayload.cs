namespace SkillSwap.Application.Common.Interfaces;

public class AppleUserPayload
{
    public string AppleId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
}
