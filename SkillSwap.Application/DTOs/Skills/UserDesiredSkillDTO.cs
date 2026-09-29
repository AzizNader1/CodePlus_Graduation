using SkillSwap.Domain.Enums;


namespace SkillSwap.Application.DTOs;

public class UserDesiredSkillDTO
{
    public Guid Id { get; set; }
    public Guid SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public ProficiencyLevel TargetLevel { get; set; }
    public SkillPriority Priority { get; set; }
    public string? Description { get; set; }
}
