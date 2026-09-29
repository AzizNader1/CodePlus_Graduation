namespace SkillSwap.Application.DTOs;

/// <summary>
/// WebRTC RTCIceServer configuration containing STUN/TURN server endpoints and credentials.
/// </summary>
public class IceServerConfigDTO
{
    public ICollection<string> Urls { get; set; } = new List<string>();
    public string? Username { get; set; }
    public string? Credential { get; set; }
}
