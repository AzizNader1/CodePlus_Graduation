namespace SkillSwap.Application.Common.Interfaces;

public interface IAppleAuthService
{
    Task<AppleUserPayload?> ValidateIdentityTokenAsync(string identityToken);
}
