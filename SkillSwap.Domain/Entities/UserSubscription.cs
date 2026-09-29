using SkillSwap.Domain.Common;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Domain.Entities;

/// <summary>
/// Active user membership subscription.
/// </summary>
public class UserSubscription : BaseAuditableEntity
{
    public Guid UserId { get; set; }
    public SubscriptionPlan Plan { get; set; } = SubscriptionPlan.Free;
    public string? StripeSubscriptionId { get; set; }
    public string Status { get; set; } = "active";
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public virtual ApplicationUser User { get; set; } = null!;
}
