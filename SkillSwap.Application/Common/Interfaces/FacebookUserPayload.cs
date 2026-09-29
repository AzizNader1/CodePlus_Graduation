namespace SkillSwap.Application.Common.Interfaces;

/// <summary>
/// Verified user profile payload extracted from Facebook Graph API.
/// </summary>
public class FacebookUserPayload
{
    public string FacebookId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PictureUrl { get; set; }
}
