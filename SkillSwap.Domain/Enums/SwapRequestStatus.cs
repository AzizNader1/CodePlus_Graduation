namespace SkillSwap.Domain.Enums;

/// <summary>
/// Life-cycle states of a skill swap negotiation proposal.
/// </summary>
public enum SwapRequestStatus
{
    Pending = 1,
    Accepted = 2,
    Rejected = 3,
    CounterOffered = 4,
    Cancelled = 5
}
