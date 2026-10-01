using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Web.Services;
using Microsoft.AspNetCore.Authentication.Google;


namespace SkillSwap.Web.Controllers;

public class AuthController : Controller
{
    private readonly ISkillSwapApiClient _apiClient;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ISkillSwapApiClient apiClient, ILogger<AuthController> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Discover");

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginRequest request, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(request);

        var response = await _apiClient.PostAsync<AuthResponseDTO>("Auth/Login", request);

        if (response == null || !response.IsSuccess || response.Data == null)
        {
            var errorMsg = response?.Message ?? "Invalid email or password.";
            if (response?.Errors != null && response.Errors.Any())
                errorMsg = string.Join("; ", response.Errors);

            ModelState.AddModelError(string.Empty, errorMsg);
            return View(request);
        }

        var auth = response.Data;

        if (auth.RequiresTwoFactor)
        {
            return RedirectToAction("TwoFactor", new { twoFactorToken = auth.TwoFactorToken });
        }

        await SignInUserAsync(auth);

        var displayName = auth.User?.FullName ?? "User";
        TempData["Success"] = $"Welcome back, {displayName}!";

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Discover");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExternalLogin(string provider, string? returnUrl = null)
    {
        ApiResponse<AuthResponseDTO>? response = null;

        if (string.Equals(provider, "Google", StringComparison.OrdinalIgnoreCase))
        {
            var request = new GoogleLoginRequest { IdToken = "google_demo_token" };
            response = await _apiClient.PostAsync<AuthResponseDTO>("Auth/GoogleLogin", request);
        }
        else if (string.Equals(provider, "Apple", StringComparison.OrdinalIgnoreCase))
        {
            var request = new AppleLoginRequest { IdentityToken = "apple_demo_token", FullName = "Sarah Chen (Apple)" };
            response = await _apiClient.PostAsync<AuthResponseDTO>("Auth/AppleLogin", request);
        }
        else if (string.Equals(provider, "Facebook", StringComparison.OrdinalIgnoreCase))
        {
            var request = new FacebookLoginRequest { AccessToken = "facebook_demo_token" };
            response = await _apiClient.PostAsync<AuthResponseDTO>("Auth/FacebookLogin", request);
        }
        else
        {
            TempData["Error"] = $"Unsupported authentication provider '{provider}'.";
            return RedirectToAction("Login", new { returnUrl });
        }

        if (response == null || !response.IsSuccess || response.Data == null)
        {
            var errorMsg = response?.Message ?? $"Failed to authenticate via {provider}.";
            TempData["Error"] = errorMsg;
            return RedirectToAction("Login", new { returnUrl });
        }

        await SignInUserAsync(response.Data);

        var displayName = response.Data.User?.FullName ?? $"{provider} User";
        TempData["Success"] = $"Successfully authenticated with {provider}! Welcome, {displayName}.";

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Discover");
    }

    [HttpGet]
    public async Task<IActionResult> ExternalLoginRedirect(string provider, string? returnUrl = null)
    {
        return await ExternalLogin(provider, returnUrl);
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Discover");

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);

        var response = await _apiClient.PostAsync<AuthResponseDTO>("Auth/Register", request);

        if (response == null || !response.IsSuccess || response.Data == null)
        {
            var errorMsg = response?.Message ?? "Registration failed.";
            if (response?.Errors != null && response.Errors.Any())
                errorMsg = string.Join("; ", response.Errors);

            ModelState.AddModelError(string.Empty, errorMsg);
            return View(request);
        }

        await SignInUserAsync(response.Data);

        TempData["Success"] = "Welcome to SkillSwap! Your account has been created successfully.";
        return RedirectToAction("Index", "Discover");
    }

    [HttpGet]
    public IActionResult TwoFactor(string? twoFactorToken)
    {
        var model = new Verify2FaRequest { TwoFactorToken = twoFactorToken };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TwoFactor(Verify2FaRequest request)
    {
        var response = await _apiClient.PostAsync<AuthResponseDTO>("Auth/Verify2Fa", request);

        if (response == null || !response.IsSuccess || response.Data == null)
        {
            ModelState.AddModelError(string.Empty, response?.Message ?? "Invalid 2FA code.");
            return View(request);
        }

        await SignInUserAsync(response.Data);
        TempData["Success"] = "Two-factor verification successful!";
        return RedirectToAction("Index", "Discover");
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);

        var response = await _apiClient.PostCommandAsync("Auth/ForgotPassword", request);

        TempData["Success"] = response?.Message ?? "If your email is registered, password reset instructions have been sent.";
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult ResetPassword(string? token, string? email)
    {
        return View(new ResetPasswordRequest { Token = token ?? string.Empty, Email = email ?? string.Empty });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);

        var response = await _apiClient.PostCommandAsync("Auth/ResetPassword", request);

        if (response == null || !response.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, response?.Message ?? "Failed to reset password.");
            return View(request);
        }

        TempData["Success"] = "Password has been reset successfully! Please log in.";
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Clear();
        TempData["Success"] = "You have been logged out safely.";
        return RedirectToAction("Index", "Home");
    }

    private async Task SignInUserAsync(AuthResponseDTO auth)
    {
        var user = auth.User;
        var role = user?.Roles.FirstOrDefault() ?? "User";

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, (user?.Id ?? Guid.Empty).ToString()),
            new(ClaimTypes.Name, user?.FullName ?? "User"),
            new(ClaimTypes.Email, user?.Email ?? string.Empty),
            new(ClaimTypes.Role, role),
            new("AccessToken", auth.AccessToken ?? string.Empty),
            new("RefreshToken", auth.RefreshToken ?? string.Empty),
            new("AvatarUrl", user?.AvatarUrl ?? string.Empty)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            });

        HttpContext.Session.SetString("AccessToken", auth.AccessToken ?? string.Empty);
    }

    [HttpGet]
public IActionResult GoogleLogin(string? returnUrl = null)
{
    var redirectUrl = Url.Action(
        nameof(GoogleCallback),
        "Auth",
        new { returnUrl });

    var properties = new AuthenticationProperties
    {
        RedirectUri = redirectUrl
    };

    return Challenge(
        properties,
        GoogleDefaults.AuthenticationScheme);
}

[HttpGet]
public async Task<IActionResult> GoogleCallback(string? returnUrl = null)
{
    var result = await HttpContext.AuthenticateAsync(
        GoogleDefaults.AuthenticationScheme);

    if (!result.Succeeded)
    {
        TempData["Error"] = "Google authentication failed.";
        return RedirectToAction("Login");
    }

    var idToken = result.Properties?.GetTokenValue("id_token");

    // if (string.IsNullOrEmpty(idToken))
    // {
    //     TempData["Error"] = "Google did not return an ID token.";
    //     return RedirectToAction("Login");
    // }

    var request = new GoogleLoginRequest
    {
        IdToken = idToken
    };

    var response = await _apiClient.PostAsync<AuthResponseDTO>(
        "Auth/GoogleLogin",
        request);

    if (response == null ||
        !response.IsSuccess ||
        response.Data == null)
    {
        TempData["Error"] =
            response?.Message ?? "Failed to authenticate with Google.";

        return RedirectToAction("Login");
    }

    await SignInUserAsync(response.Data);

    var displayName =
        response.Data.User?.FullName ?? "Google User";

    TempData["Success"] =
        $"Welcome, {displayName}!";

    if (!string.IsNullOrEmpty(returnUrl) &&
        Url.IsLocalUrl(returnUrl))
    {
        return Redirect(returnUrl);
    }

    return RedirectToAction("Index", "Discover");
}

}
