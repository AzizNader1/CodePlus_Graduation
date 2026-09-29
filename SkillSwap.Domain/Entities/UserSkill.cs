using SkillSwap.Domain.Common;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Domain.Entities;

/// <summary>
/// A skill offered/taught by a user, including experience, proficiency, and portfolio evidence.
/// </summary>
public class UserSkill : BaseAuditableEntity, ISoftDelete
{
    public Guid UserId { get; set; }
    public Guid SkillId { get; set; }
    public ProficiencyLevel ProficiencyLevel { get; set; } = ProficiencyLevel.Intermediate;
    public int YearsOfExperience { get; set; } = 1;
    public string? PortfolioUrl { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual Skill Skill { get; set; } = null!;
}
