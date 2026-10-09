using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Web.Extensions;
using SkillSwap.Web.Middlewares;
using SkillSwap.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Load private local configurations if available (ignored by git)
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// 1. Forwarded Headers for MonsterASP / Reverse Proxy Hosting
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// 2. MVC & Razor Views with Global Antiforgery Protection
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

var cookieSecurePolicy = builder.Environment.IsDevelopment()
    ? CookieSecurePolicy.SameAsRequest
    : CookieSecurePolicy.Always;

// 3. Antiforgery Cookie Security
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = cookieSecurePolicy;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddHttpContextAccessor();

// 4. Secure Session Management
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = cookieSecurePolicy;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// 5. Secure Cookie Authentication & External OAuth Providers
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = cookieSecurePolicy;
        options.Cookie.SameSite = SameSiteMode.Lax;
    })
    .AddCookie("ExternalAuth", options =>
    {
        options.Cookie.Name = ".SkillSwap.ExternalAuth";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = cookieSecurePolicy;
        options.Cookie.SameSite = SameSiteMode.Lax;
    })
    .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
    {
        options.SignInScheme = "ExternalAuth";
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "placeholder";
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "placeholder";
        options.SaveTokens = true;
        options.Events.OnCreatingTicket = ctx =>
        {
            if (ctx.TokenResponse?.Response != null &&
                ctx.TokenResponse.Response.RootElement.ValueKind == System.Text.Json.JsonValueKind.Object &&
                ctx.TokenResponse.Response.RootElement.TryGetProperty("id_token", out var idTokenElement))
            {
                var idToken = idTokenElement.GetString();
                if (!string.IsNullOrEmpty(idToken) && ctx.Properties != null)
                {
                    var tokens = ctx.Properties.GetTokens().ToList();
                    tokens.RemoveAll(t => t.Name == "id_token");
                    tokens.Add(new AuthenticationToken { Name = "id_token", Value = idToken });
                    ctx.Properties.StoreTokens(tokens);
                }
            }
            return Task.CompletedTask;
        };
    });

// 6. HSTS Security for Production
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

// 7. Typed HttpClient for SkillSwap API (Default fallback to live production server)
var apiBaseUrl = builder.Configuration["SkillSwapApi:BaseUrl"] ?? "https://skillswapapi.runasp.net/api/v1/";
builder.Services.AddHttpClient<ISkillSwapApiClient, SkillSwapApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Initialize Avatar resolver with API base domain
AvatarHelper.Initialize(apiBaseUrl);

var app = builder.Build();

// 8. Reverse Proxy Forwarding
app.UseForwardedHeaders();

// 9. Production Security Headers
app.UseProductionSecurityHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
