using SkillSwap.Domain.Enums;


namespace SkillSwap.Application.DTOs;

public class Verify2FaRequest
{
    public string? TwoFactorToken { get; set; }
    public string Code { get; set; } = string.Empty;
}
