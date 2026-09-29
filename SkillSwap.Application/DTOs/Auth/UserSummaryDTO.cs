using SkillSwap.Domain.Enums;


namespace SkillSwap.Application.DTOs;

public class UserSummaryDTO
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public double AverageRating { get; set; }
    public int TotalSwapsCompleted { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
}
