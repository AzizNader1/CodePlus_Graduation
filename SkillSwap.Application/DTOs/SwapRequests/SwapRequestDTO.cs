using SkillSwap.Domain.Enums;


namespace SkillSwap.Application.DTOs;

public class SwapRequestDTO
{
    public Guid Id { get; set; }
    public Guid RequesterId { get; set; }
    public string RequesterName { get; set; } = string.Empty;
    public string? RequesterAvatarUrl { get; set; }
    public double RequesterRating { get; set; }

    public Guid ReceiverId { get; set; }
    public string ReceiverName { get; set; } = string.Empty;
    public string? ReceiverAvatarUrl { get; set; }
    public double ReceiverRating { get; set; }

    public Guid OfferedSkillId { get; set; }
    public string OfferedSkillName { get; set; } = string.Empty;

    public Guid RequestedSkillId { get; set; }
    public string RequestedSkillName { get; set; } = string.Empty;

    public DateTime ProposedDate { get; set; }
    public int DurationMinutes { get; set; }
    public SwapRequestStatus Status { get; set; }

    public string? Notes { get; set; }
    public string? RejectionReason { get; set; }

    public DateTime? CounterProposedDate { get; set; }
    public int? CounterDurationMinutes { get; set; }
    public string? CounterNotes { get; set; }

    public DateTime CreatedAt { get; set; }
}
