using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.DTOs;

/// <summary>
/// Response payload returned when a verified participant joins a scheduled video call session.
/// </summary>
public class JoinSessionCallResponseDTO
{
    public Guid SessionId { get; set; }
    public string RoomId { get; set; } = string.Empty;
    public string RoomSecurityToken { get; set; } = string.Empty;
    public string ParticipantRole { get; set; } = string.Empty; // "Host" or "Participant"
    public string PartnerName { get; set; } = string.Empty;
    public string? PartnerAvatarUrl { get; set; }
    public Guid PartnerUserId { get; set; }
    public DateTime ScheduledStartTime { get; set; }
    public DateTime ScheduledEndTime { get; set; }
    public int DurationMinutes { get; set; }
    public SessionStatus Status { get; set; }
    public ICollection<IceServerConfigDTO> IceServers { get; set; } = new List<IceServerConfigDTO>();
}
