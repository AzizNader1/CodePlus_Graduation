using SkillSwap.Domain.Enums;


namespace SkillSwap.Application.DTOs;

public class PublicUserProfileDTO
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Location { get; set; }
    public double AverageRating { get; set; }
    public int TotalReviewsCount { get; set; }
    public int TotalSwapsCompleted { get; set; }

    public ICollection<UserSkillDTO> SkillsOffered { get; set; } = new List<UserSkillDTO>();
    public ICollection<UserDesiredSkillDTO> SkillsWanted { get; set; } = new List<UserDesiredSkillDTO>();
    public ICollection<UserAvailabilityDTO> Availabilities { get; set; } = new List<UserAvailabilityDTO>();
    public ICollection<ReviewDTO> RecentReviews { get; set; } = new List<ReviewDTO>();
}
