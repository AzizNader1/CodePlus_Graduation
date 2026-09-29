using SkillSwap.Domain.Enums;


namespace SkillSwap.Application.DTOs;

public class AppleLoginRequest
{
    public string IdentityToken { get; set; } = string.Empty;
    public string? FullName { get; set; }
}
