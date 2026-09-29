using SkillSwap.Domain.Common;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Domain.Entities;

/// <summary>
/// A confirmed, scheduled swap session resulting from an accepted swap request.
/// </summary>
public class SwapSession : BaseAuditableEntity, ISoftDelete
{
    public Guid SwapRequestId { get; set; }
    public Guid HostUserId { get; set; }
    public Guid ParticipantUserId { get; set; }

    public DateTime ScheduledStartTime { get; set; }
    public DateTime ScheduledEndTime { get; set; }
    public int DurationMinutes { get; set; } = 60;
    public string? MeetingLink { get; set; }

    public SessionStatus Status { get; set; } = SessionStatus.Scheduled;

    public string? HostNotes { get; set; }
    public string? ParticipantNotes { get; set; }

    public bool HostConfirmedCompleted { get; set; } = false;
    public bool ParticipantConfirmedCompleted { get; set; } = false;
    public DateTime? CompletedAt { get; set; }

    public string? CancellationReason { get; set; }
    public Guid? CancelledByUserId { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Video Call Telemetry & Attendance Tracking
    public DateTime? ActualStartTime { get; set; }
    public DateTime? ActualEndTime { get; set; }
    public int? ActualDurationMinutes { get; set; }

    public DateTime? HostJoinedAt { get; set; }
    public DateTime? ParticipantJoinedAt { get; set; }
    public DateTime? HostLastHeartbeatAt { get; set; }
    public DateTime? ParticipantLastHeartbeatAt { get; set; }

    public bool IsHostInCall { get; set; } = false;
    public bool IsParticipantInCall { get; set; } = false;

    public string? RoomSecurityToken { get; set; }

    // Navigation
    public virtual SwapRequest SwapRequest { get; set; } = null!;
    public virtual ApplicationUser HostUser { get; set; } = null!;
    public virtual ApplicationUser ParticipantUser { get; set; } = null!;
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
