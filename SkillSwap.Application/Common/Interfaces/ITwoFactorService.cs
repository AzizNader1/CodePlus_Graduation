namespace SkillSwap.Application.Common.Interfaces;

public interface ITwoFactorService
{
    string GenerateSecretKey();
    string GenerateQrCodeUri(string email, string secretKey);
    bool VerifyTotpCode(string secretKey, string code);
    IEnumerable<string> GenerateRecoveryCodes(int count = 10);
}
