using SkillSwap.Domain.Enums;


namespace SkillSwap.Application.DTOs;

public class GoogleLoginRequest
{
    public string IdToken { get; set; } = string.Empty;
}
