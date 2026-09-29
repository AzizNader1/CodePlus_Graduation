using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.DTOs;

/// <summary>
/// Status payload indicating real-time in-call attendance and connection state.
/// </summary>
public class SessionCallStatusDTO
{
    public Guid SessionId { get; set; }
    public bool IsHostInCall { get; set; }
    public bool IsParticipantInCall { get; set; }
    public DateTime? HostJoinedAt { get; set; }
    public DateTime? ParticipantJoinedAt { get; set; }
    public DateTime? HostLastHeartbeatAt { get; set; }
    public DateTime? ParticipantLastHeartbeatAt { get; set; }
    public int ElapsedCallMinutes { get; set; }
    public int? ActualDurationMinutes { get; set; }
    public SessionStatus Status { get; set; }
}
