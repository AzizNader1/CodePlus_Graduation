namespace SkillSwap.Domain.Enums;

/// <summary>
/// Life-cycle status of an active or completed swap session.
/// </summary>
public enum SessionStatus
{
    Scheduled = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4,
    NoShow = 5
}
