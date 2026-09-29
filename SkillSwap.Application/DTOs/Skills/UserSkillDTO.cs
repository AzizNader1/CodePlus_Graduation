using SkillSwap.Domain.Enums;


namespace SkillSwap.Application.DTOs;

public class UserSkillDTO
{
    public Guid Id { get; set; }
    public Guid SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public ProficiencyLevel ProficiencyLevel { get; set; }
    public int YearsOfExperience { get; set; }
    public string? PortfolioUrl { get; set; }
    public string? Description { get; set; }
}
