using SkillSwap.Domain.Common;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Domain.Entities;

/// <summary>
/// User report or safety dispute submitted for administrator investigation.
/// </summary>
public class Report : BaseAuditableEntity
{
    public Guid ReporterId { get; set; }
    public Guid ReportedUserId { get; set; }
    public Guid? SessionId { get; set; }

    public string ReasonCategory { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public ReportStatus Status { get; set; } = ReportStatus.Pending;
    public string? AdminNotes { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public Guid? ResolvedByAdminId { get; set; }

    // Navigation
    public virtual ApplicationUser Reporter { get; set; } = null!;
    public virtual ApplicationUser ReportedUser { get; set; } = null!;
    public virtual SwapSession? Session { get; set; }
}
