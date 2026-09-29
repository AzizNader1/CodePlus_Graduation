namespace SkillSwap.Application.DTOs;

/// <summary>
/// Payload submitted for Facebook OAuth 2.0 access token authentication.
/// </summary>
public class FacebookLoginRequest
{
    public string AccessToken { get; set; } = string.Empty;
}
