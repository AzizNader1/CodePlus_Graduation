using SkillSwap.Domain.Enums;


namespace SkillSwap.Application.DTOs;

public class UpdateProfileRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? Location { get; set; }
    public string? TimeZone { get; set; }
}
