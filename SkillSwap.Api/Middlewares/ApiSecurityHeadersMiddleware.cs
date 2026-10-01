namespace SkillSwap.Api.Middlewares;

public class ApiSecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public ApiSecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;

            // 1. Prevent MIME Sniffing
            headers["X-Content-Type-Options"] = "nosniff";

            // 2. Prevent Framing / Clickjacking
            headers["X-Frame-Options"] = "DENY";

            // 3. XSS Filter Protection
            headers["X-XSS-Protection"] = "1; mode=block";

            // 4. Strict Referrer Policy
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

            // 5. Restrict API Framing and Execution (Permit swagger UI if on swagger route)
            if (!context.Request.Path.StartsWithSegments("/swagger") && context.Request.Path != "/")
            {
                headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";
            }

            // 6. Strip Server Fingerprints to prevent reconnaissance
            headers.Remove("Server");
            headers.Remove("X-Powered-By");
            headers.Remove("X-AspNet-Version");
            headers.Remove("X-AspNetMvc-Version");

            return Task.CompletedTask;
        });

        await _next(context);
    }
}

public static class ApiSecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseApiSecurityHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ApiSecurityHeadersMiddleware>();
    }
}
