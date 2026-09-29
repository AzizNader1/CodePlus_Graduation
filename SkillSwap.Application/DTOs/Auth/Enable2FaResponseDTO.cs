using SkillSwap.Domain.Enums;


namespace SkillSwap.Application.DTOs;

public class Enable2FaResponseDTO
{
    public string SecretKey { get; set; } = string.Empty;
    public string QrCodeUri { get; set; } = string.Empty;
    public IEnumerable<string> RecoveryCodes { get; set; } = new List<string>();
}
