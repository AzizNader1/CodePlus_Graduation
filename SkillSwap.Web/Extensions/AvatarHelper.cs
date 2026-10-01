namespace SkillSwap.Web.Extensions;

public static class AvatarHelper
{
    private static string _apiHost = "https://skillswapapi.runasp.net";

    public static void Initialize(string? apiBaseUrl)
    {
        if (!string.IsNullOrWhiteSpace(apiBaseUrl) && Uri.TryCreate(apiBaseUrl, UriKind.Absolute, out var uri))
        {
            _apiHost = $"{uri.Scheme}://{uri.Authority}".TrimEnd('/');
        }
    }

    /// <summary>
    /// Formats avatar URL for production display:
    /// - If absolute (http:// or https://), returns as is (Google, external CDN, Unsplash).
    /// - If relative (/avatars/filename.png), prepends the configured API host URL.
    /// - If empty or null, returns a fallback default avatar asset.
    /// </summary>
    public static string ToAvatarUrl(this string? avatarUrl)
    {
        if (string.IsNullOrWhiteSpace(avatarUrl))
        {
            return "/images/default-avatar.svg";
        }

        var trimmed = avatarUrl.Trim();

        if (trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return trimmed;
        }

        return $"{_apiHost}/{(trimmed.TrimStart('/'))}";
    }
}
