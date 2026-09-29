using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.DTOs;

namespace SkillSwap.Infrastructure.Services;

public class VideoCallService : IVideoCallService
{
    private readonly IConfiguration _configuration;

    public VideoCallService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateRoomToken(Guid sessionId, Guid userId, string role)
    {
        var secret = _configuration["VideoCall:TokenSecret"]
                     ?? _configuration["Jwt:SecretKey"]
                     ?? "SkillSwapVideoCallSecretKeyTokenSigningKey2026!";
        var expiry = DateTimeOffset.UtcNow.AddHours(4).ToUnixTimeSeconds();
        var rawPayload = $"{sessionId:N}:{userId:N}:{role}:{expiry}";
        var payloadBytes = Encoding.UTF8.GetBytes(rawPayload);
        var base64Payload = Convert.ToBase64String(payloadBytes);

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(base64Payload));
        var base64Sig = Convert.ToBase64String(signatureBytes);

        return $"{base64Payload}.{base64Sig}";
    }

    public bool ValidateRoomToken(Guid sessionId, Guid userId, string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        var parts = token.Split('.');
        if (parts.Length != 2)
            return false;

        var base64Payload = parts[0];
        var providedSig = parts[1];

        var secret = _configuration["VideoCall:TokenSecret"]
                     ?? _configuration["Jwt:SecretKey"]
                     ?? "SkillSwapVideoCallSecretKeyTokenSigningKey2026!";

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var expectedSigBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(base64Payload));
        var expectedSig = Convert.ToBase64String(expectedSigBytes);

        if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(providedSig), Encoding.UTF8.GetBytes(expectedSig)))
            return false;

        try
        {
            var payloadString = Encoding.UTF8.GetString(Convert.FromBase64String(base64Payload));
            var segments = payloadString.Split(':');
            if (segments.Length != 4)
                return false;

            if (!Guid.TryParse(segments[0], out var tokenSessionId) || tokenSessionId != sessionId)
                return false;

            if (!Guid.TryParse(segments[1], out var tokenUserId) || tokenUserId != userId)
                return false;

            if (!long.TryParse(segments[3], out var expiryUnix))
                return false;

            var expiryTime = DateTimeOffset.FromUnixTimeSeconds(expiryUnix);
            if (DateTimeOffset.UtcNow > expiryTime)
                return false;

            return true;
        }
        catch
        {
            return false;
        }
    }

    public IReadOnlyList<IceServerConfigDTO> GetIceServers()
    {
        var servers = new List<IceServerConfigDTO>
        {
            new IceServerConfigDTO
            {
                Urls = new List<string>
                {
                    "stun:stun.l.google.com:19302",
                    "stun:stun1.l.google.com:19302",
                    "stun:stun2.l.google.com:19302"
                }
            }
        };

        var turnUrl = _configuration["WebRtc:TurnServer:Url"];
        var turnUser = _configuration["WebRtc:TurnServer:Username"];
        var turnCred = _configuration["WebRtc:TurnServer:Credential"];

        if (!string.IsNullOrEmpty(turnUrl))
        {
            servers.Add(new IceServerConfigDTO
            {
                Urls = new List<string> { turnUrl },
                Username = turnUser,
                Credential = turnCred
            });
        }

        return servers;
    }
}
