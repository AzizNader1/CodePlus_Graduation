using SkillSwap.Application.DTOs;

namespace SkillSwap.Application.Common.Interfaces;

/// <summary>
/// Service contract for video room authorization and WebRTC ICE server configurations.
/// </summary>
public interface IVideoCallService
{
    string GenerateRoomToken(Guid sessionId, Guid userId, string role);
    bool ValidateRoomToken(Guid sessionId, Guid userId, string token);
    IReadOnlyList<IceServerConfigDTO> GetIceServers();
}
