using SkillSwap.Domain.Enums;


namespace SkillSwap.Application.DTOs;

public class CreateReviewRequest
{
    public Guid SessionId { get; set; }
    public int OverallRating { get; set; } = 5;
    public int PunctualityScore { get; set; } = 5;
    public int CommunicationScore { get; set; } = 5;
    public int KnowledgeScore { get; set; } = 5;
    public string? Comment { get; set; }
}
