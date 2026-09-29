using SkillSwap.Domain.Enums;


namespace SkillSwap.Application.DTOs;

public class AddOfferedSkillRequest
{
    public Guid SkillId { get; set; }
    public ProficiencyLevel ProficiencyLevel { get; set; } = ProficiencyLevel.Intermediate;
    public int YearsOfExperience { get; set; } = 1;
    public string? PortfolioUrl { get; set; }
    public string? Description { get; set; }
}
