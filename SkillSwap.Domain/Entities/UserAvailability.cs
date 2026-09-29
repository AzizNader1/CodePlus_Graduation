using SkillSwap.Domain.Common;

namespace SkillSwap.Domain.Entities;

/// <summary>
/// Weekly recurring availability time slot for skill swapping.
/// </summary>
public class UserAvailability : BaseEntity
{
    public Guid UserId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsRecurring { get; set; } = true;

    // Navigation
    public virtual ApplicationUser User { get; set; } = null!;
}
