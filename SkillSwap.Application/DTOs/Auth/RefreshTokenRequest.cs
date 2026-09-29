using SkillSwap.Domain.Enums;


namespace SkillSwap.Application.DTOs;

public class RefreshTokenRequest
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
