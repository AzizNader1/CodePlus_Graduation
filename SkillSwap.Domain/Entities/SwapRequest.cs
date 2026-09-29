using SkillSwap.Domain.Common;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Domain.Entities;

/// <summary>
/// A bilateral proposal sent from one user to another to trade skills.
/// </summary>
public class SwapRequest : BaseAuditableEntity, ISoftDelete
{
    public Guid RequesterId { get; set; }
    public Guid ReceiverId { get; set; }
    public Guid OfferedSkillId { get; set; }
    public Guid RequestedSkillId { get; set; }

    public DateTime ProposedDate { get; set; }
    public int DurationMinutes { get; set; } = 60;
    public SwapRequestStatus Status { get; set; } = SwapRequestStatus.Pending;

    public string? Notes { get; set; }
    public string? RejectionReason { get; set; }

    // Counter offer details
    public DateTime? CounterProposedDate { get; set; }
    public int? CounterDurationMinutes { get; set; }
    public string? CounterNotes { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation
    public virtual ApplicationUser Requester { get; set; } = null!;
    public virtual ApplicationUser Receiver { get; set; } = null!;
    public virtual Skill OfferedSkill { get; set; } = null!;
    public virtual Skill RequestedSkill { get; set; } = null!;
    public virtual SwapSession? Session { get; set; }
}
