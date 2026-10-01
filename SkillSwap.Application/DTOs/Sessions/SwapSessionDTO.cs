using SkillSwap.Domain.Enums;


namespace SkillSwap.Application.DTOs;

public class SwapSessionDTO
{
    public Guid Id { get; set; }
    public Guid SwapRequestId { get; set; }

    public Guid HostUserId { get; set; }
    public string HostName { get; set; } = string.Empty;
    public string? HostAvatarUrl { get; set; }

    public Guid ParticipantUserId { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public string? ParticipantAvatarUrl { get; set; }

    public DateTime ScheduledStartTime { get; set; }
    public DateTime ScheduledEndTime { get; set; }
    public int DurationMinutes { get; set; }
    public string? MeetingLink { get; set; }
    public SessionStatus Status { get; set; }

    public bool HostConfirmedCompleted { get; set; }
    public bool ParticipantConfirmedCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool HasCurrentUserReviewed { get; set; }
}
