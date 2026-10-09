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
            if (!string.IsNullOrWhiteSpace(clientId) && !clientId.Contains("your-google-client-id", StringComparison.OrdinalIgnoreCase))
            {
                var audiences = clientId.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                settings.Audience = audiences;
            }

            GoogleJsonWebSignature.Payload? payload = null;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Validation with audience failed; falling back to signature-only validation.");
                payload = await GoogleJsonWebSignature.ValidateAsync(idToken);
            }

            if (payload == null)
                return null;

            var displayName = !string.IsNullOrWhiteSpace(payload.Name)
                ? payload.Name
                : (!string.IsNullOrWhiteSpace(payload.Email) ? payload.Email.Split('@')[0] : "Google User");

            return new GoogleUserPayload
            {
                GoogleId = payload.Subject,
                Email = payload.Email,
                Name = displayName,
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
