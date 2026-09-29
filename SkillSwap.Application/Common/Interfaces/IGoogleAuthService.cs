namespace SkillSwap.Application.Common.Interfaces;

public interface IGoogleAuthService
{
    Task<GoogleUserPayload?> ValidateIdTokenAsync(string idToken);
}
