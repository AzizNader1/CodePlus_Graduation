namespace SkillSwap.Domain.Enums;

/// <summary>
/// Stripe payment processing statuses.
/// </summary>
public enum PaymentStatus
{
    Pending = 1,
    Succeeded = 2,
    Failed = 3,
    Refunded = 4
}
