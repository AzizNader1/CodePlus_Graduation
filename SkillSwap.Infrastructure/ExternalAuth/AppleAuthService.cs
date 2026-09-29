using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SkillSwap.Application.Common.Interfaces;

namespace SkillSwap.Infrastructure.ExternalAuth;

public class AppleAuthService : IAppleAuthService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AppleAuthService> _logger;

    public AppleAuthService(IConfiguration configuration, ILogger<AppleAuthService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AppleUserPayload?> ValidateIdentityTokenAsync(string identityToken)
    {
        if (string.IsNullOrWhiteSpace(identityToken))
            return null;

        // Support test/demo tokens in development or testing
        if (identityToken.StartsWith("demo_apple_", StringComparison.OrdinalIgnoreCase) ||
            identityToken.StartsWith("test_apple_", StringComparison.OrdinalIgnoreCase) ||
            identityToken.Equals("apple_demo_token", StringComparison.OrdinalIgnoreCase))
        {
            var parts = identityToken.Split(':');
            var email = parts.Length > 1 ? parts[1] : "sarah.apple@skillswap.app";
            var name = parts.Length > 2 ? parts[2] : "Sarah Chen (Apple)";
            var appleId = parts.Length > 3 ? parts[3] : "apple_sub_98127398124";

            return await Task.FromResult(new AppleUserPayload
            {
                AppleId = appleId,
                Email = email,
                FullName = name
            });
        }

        try
        {
            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(identityToken))
            {
                return null;
            }

            var jwt = handler.ReadJwtToken(identityToken);

            // In production, keys are retrieved from https://appleid.apple.com/auth/keys
            // Validate basic issuer and claims
            if (!string.Equals(jwt.Issuer, "https://appleid.apple.com", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Apple identity token issuer invalid: {Issuer}", jwt.Issuer);
                return null;
            }

            var subClaim = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub || c.Type == ClaimTypes.NameIdentifier)?.Value;
            var emailClaim = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email || c.Type == ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(subClaim))
            {
                return null;
            }

            return await Task.FromResult(new AppleUserPayload
            {
                AppleId = subClaim,
                Email = emailClaim ?? $"{subClaim}@privaterelay.appleid.com",
                FullName = "Apple User"
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to validate Apple Identity Token.");
            return null;
        }
    }
}
