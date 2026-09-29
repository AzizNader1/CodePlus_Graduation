using SkillSwap.Domain.Enums;


namespace SkillSwap.Application.DTOs;

public class ReviewDTO
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid ReviewerId { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public string? ReviewerAvatarUrl { get; set; }
    public int OverallRating { get; set; }
    public int PunctualityScore { get; set; }
    public int CommunicationScore { get; set; }
    public int KnowledgeScore { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}
