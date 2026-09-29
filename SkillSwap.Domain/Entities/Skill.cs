using SkillSwap.Domain.Common;

namespace SkillSwap.Domain.Entities;

/// <summary>
/// Specific individual skill (e.g., C# / .NET, Python, Spanish, Guitar, UI/UX).
/// </summary>
public class Skill : BaseAuditableEntity, ISoftDelete
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation
    public virtual Category Category { get; set; } = null!;
    public virtual ICollection<UserSkill> UserSkills { get; set; } = new List<UserSkill>();
    public virtual ICollection<UserDesiredSkill> DesiredByUsers { get; set; } = new List<UserDesiredSkill>();
}
