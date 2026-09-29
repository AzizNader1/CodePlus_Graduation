using SkillSwap.Domain.Enums;


namespace SkillSwap.Application.DTOs;

public class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}
