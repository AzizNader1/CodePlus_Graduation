namespace SkillSwap.Application.DTOs;

/// <summary>
/// Lightweight in-call keepalive heartbeat request payload.
/// </summary>
public class SessionHeartbeatRequest
{
    public bool IsAudioMuted { get; set; } = false;
    public bool IsVideoMuted { get; set; } = false;
    public bool IsScreenSharing { get; set; } = false;
}
