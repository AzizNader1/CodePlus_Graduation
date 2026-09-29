using System.Security.Cryptography;
using OtpNet;
using QRCoder;
using SkillSwap.Application.Common.Interfaces;

namespace SkillSwap.Infrastructure.Identity;

public class TwoFactorService : ITwoFactorService
{
    public string GenerateSecretKey()
    {
        var key = KeyGeneration.GenerateRandomKey(20);
        return Base32Encoding.ToString(key);
    }

    public string GenerateQrCodeUri(string email, string secretKey)
    {
        var appName = "SkillSwap";
        var otpUri = $"otpauth://totp/{appName}:{email}?secret={secretKey}&issuer={appName}&digits=6";

        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(otpUri, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        var qrCodeBytes = qrCode.GetGraphic(20);

        return $"data:image/png;base64,{Convert.ToBase64String(qrCodeBytes)}";
    }

    public bool VerifyTotpCode(string secretKey, string code)
    {
        try
        {
            var keyBytes = Base32Encoding.ToBytes(secretKey);
            var totp = new Totp(keyBytes);
            return totp.VerifyTotp(code, out _, VerificationWindow.RfcSpecifiedNetworkDelay);
        }
        catch
        {
            return false;
        }
    }

    public IEnumerable<string> GenerateRecoveryCodes(int count = 10)
    {
        var codes = new List<string>();
        for (int i = 0; i < count; i++)
        {
            var bytes = new byte[5];
            RandomNumberGenerator.Fill(bytes);
            codes.Add(Convert.ToHexString(bytes).ToLowerInvariant());
        }
        return codes;
    }
}
