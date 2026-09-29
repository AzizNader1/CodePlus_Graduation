namespace SkillSwap.Domain.Enums;

/// <summary>
/// Moderation status for safety and dispute reports.
/// </summary>
public enum ReportStatus
{
    Pending = 1,
    UnderInvestigation = 2,
    Resolved = 3,
    Dismissed = 4
}
