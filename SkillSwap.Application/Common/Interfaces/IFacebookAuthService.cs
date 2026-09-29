namespace SkillSwap.Application.Common.Interfaces;

/// <summary>
/// Service contract for validating Facebook OAuth 2.0 user access tokens.
/// </summary>
public interface IFacebookAuthService
{
    Task<FacebookUserPayload?> ValidateAccessTokenAsync(string accessToken);
}
