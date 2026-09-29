using SkillSwap.Domain.Enums;


namespace SkillSwap.Application.DTOs;

public class RegisterRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? TimeZone { get; set; }
}
