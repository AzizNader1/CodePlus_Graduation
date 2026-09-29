using SkillSwap.Domain.Enums;


namespace SkillSwap.Application.DTOs;

public class AuthResponseDTO
{
    public bool RequiresTwoFactor { get; set; } = false;
    public string? TwoFactorToken { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public UserSummaryDTO? User { get; set; }
}
