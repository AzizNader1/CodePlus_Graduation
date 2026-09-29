using SkillSwap.Domain.Enums;


namespace SkillSwap.Application.DTOs;

public class UserProfileDTO
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Location { get; set; }
    public string? TimeZone { get; set; }
    public double AverageRating { get; set; }
    public int TotalReviewsCount { get; set; }
    public int TotalSwapsCompleted { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<UserSkillDTO> SkillsOffered { get; set; } = new List<UserSkillDTO>();
    public ICollection<UserDesiredSkillDTO> SkillsWanted { get; set; } = new List<UserDesiredSkillDTO>();
    public ICollection<UserAvailabilityDTO> Availabilities { get; set; } = new List<UserAvailabilityDTO>();
}
