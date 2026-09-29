using SkillSwap.Domain.Enums;


namespace SkillSwap.Application.DTOs;

public class AddWantedSkillRequest
{
    public Guid SkillId { get; set; }
    public ProficiencyLevel TargetLevel { get; set; } = ProficiencyLevel.Beginner;
    public SkillPriority Priority { get; set; } = SkillPriority.Medium;
    public string? Description { get; set; }
}
