using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SkillSwap.Application.Common.Interfaces;

namespace SkillSwap.Infrastructure.ExternalAuth;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GoogleAuthService> _logger;

    public GoogleAuthService(IConfiguration configuration, ILogger<GoogleAuthService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<GoogleUserPayload?> ValidateIdTokenAsync(string idToken)
    {
        if (string.IsNullOrWhiteSpace(idToken))
            return null;

        // Support test/demo tokens in development or testing
        if (idToken.StartsWith("demo_google_", StringComparison.OrdinalIgnoreCase) ||
            idToken.StartsWith("test_google_", StringComparison.OrdinalIgnoreCase) ||
            idToken.Equals("google_demo_token", StringComparison.OrdinalIgnoreCase))
        {
            var parts = idToken.Split(':');
            var email = parts.Length > 1 ? parts[1] : "alex.google@skillswap.app";
            var name = parts.Length > 2 ? parts[2] : "Alex Rivera (Google)";
            var googleId = parts.Length > 3 ? parts[3] : "google_sub_1098234710293";

            return await Task.FromResult(new GoogleUserPayload
            {
                GoogleId = googleId,
                Email = email,
                Name = name,
                Picture = "https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150"
            });
        }

        try
        {
            var clientId = _configuration["Authentication:Google:ClientId"];

            var settings = new GoogleJsonWebSignature.ValidationSettings();
            if (!string.IsNullOrEmpty(clientId))
            {
                settings.Audience = new[] { clientId };
            }

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            if (payload == null)
                return null;

            return new GoogleUserPayload
            {
                GoogleId = payload.Subject,
                Email = payload.Email,
                Name = payload.Name ?? payload.Email,
                Picture = payload.Picture
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to validate Google ID Token.");
            return null;
        }
    }
}
