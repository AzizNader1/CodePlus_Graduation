namespace SkillSwap.Web.Middlewares;

public class ProductionSecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public ProductionSecurityHeadersMiddleware(RequestDelegate next)
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

            // 2. Prevent Clickjacking
            headers["X-Frame-Options"] = "SAMEORIGIN";

            // 3. XSS Filter Protection
            headers["X-XSS-Protection"] = "1; mode=block";

            // 4. Strict Referrer Policy
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

            // 5. Restrict Browser Features (Permit Camera & Microphone for WebRTC video exchange)
            headers["Permissions-Policy"] = "camera=(self), microphone=(self), display-capture=(self), geolocation=()";

            // 6. Strip Server Fingerprints
            headers.Remove("Server");
            headers.Remove("X-Powered-By");
            headers.Remove("X-AspNet-Version");
            headers.Remove("X-AspNetMvc-Version");

            return Task.CompletedTask;
        });

        await _next(context);
    }
}

public static class ProductionSecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseProductionSecurityHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ProductionSecurityHeadersMiddleware>();
    }
}
