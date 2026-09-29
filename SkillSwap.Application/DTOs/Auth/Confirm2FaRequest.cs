namespace SkillSwap.Application.DTOs;

/// <summary>
/// Request payload for confirming 2FA setup with a 6-digit TOTP code.
/// </summary>
public class Confirm2FaRequest
{
    public string Code { get; set; } = string.Empty;
}
