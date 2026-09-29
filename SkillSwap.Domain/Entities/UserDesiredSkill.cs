using SkillSwap.Domain.Common;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Domain.Entities;

/// <summary>
/// A skill a user desires to learn, with priority and target proficiency.
/// </summary>
public class UserDesiredSkill : BaseAuditableEntity
{
    public Guid UserId { get; set; }
    public Guid SkillId { get; set; }
    public ProficiencyLevel TargetLevel { get; set; } = ProficiencyLevel.Beginner;
    public SkillPriority Priority { get; set; } = SkillPriority.Medium;
    public string? Description { get; set; }

    // Navigation
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual Skill Skill { get; set; } = null!;
}
