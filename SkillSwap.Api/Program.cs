using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using SkillSwap.Api.Data;
using SkillSwap.Api.Middlewares;
using SkillSwap.Application;
using SkillSwap.Application.Common.Models;
using SkillSwap.Infrastructure;
using SkillSwap.Infrastructure.Hubs;

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

// 2. Add Clean Architecture Layers
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// 3. Add API Controllers & JSON Serialization
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
            .ToArray();

        var response = ApiResponse.Failure(
            message: errors.FirstOrDefault() ?? "Validation failed.",
            errors: errors,
            statusCode: StatusCodes.Status400BadRequest);

        return new BadRequestObjectResult(response);
    };
});

// 4. Enterprise Rate Limiting Protection (Anti-Brute Force & Anti-DDoS)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Global sliding window: 150 requests per minute per IP
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetSlidingWindowLimiter(clientIp, _ => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = 150,
            Window = TimeSpan.FromMinutes(1),
            SegmentsPerWindow = 6,
            QueueLimit = 10,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
    });

    // Strict limiter for authentication endpoints: 15 requests per minute
    options.AddPolicy("AuthRateLimit", context =>
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 15,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 2
        });
    });
});

// 5. Hardened Production CORS Policy
var configuredOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "https://skillswapwebsite.runasp.net", "http://skillswapwebsite.runasp.net" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("ProductionCorsPolicy", policy =>
    {
        policy.WithOrigins(configuredOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });

    options.AddPolicy("DevelopmentCorsPolicy", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
        {
            if (string.IsNullOrWhiteSpace(origin)) return false;
            if (configuredOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase)) return true;
            if (Uri.TryCreate(origin, UriKind.Absolute, out var uri) && uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        })
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

// 6. HSTS Security
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

// 7. Configure Swagger / OpenAPI with JWT Authorization Support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Skill Swap Web API",
        Version = "v1",
        Description = "Enterprise-Grade Peer-to-Peer Skill Sharing & Swap Negotiation Platform API built with Clean Architecture, CQRS, SignalR, and SQL Server."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT Bearer token in the format: Bearer {your token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// 8. Automatic Database Migration & Seeding
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        logger.LogInformation("Initializing database and applying migrations...");
        await DbInitializer.InitializeAsync(app.Services);
        logger.LogInformation("Database initialized and seeded successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

// 9. Reverse Proxy Forwarding
app.UseForwardedHeaders();

// 10. HTTP Pipeline Middlewares & Enterprise Defenses
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseApiSecurityHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Skill Swap API v1");
    c.RoutePrefix = string.Empty;
});

app.UseCors(app.Environment.IsDevelopment() ? "DevelopmentCorsPolicy" : "ProductionCorsPolicy");

app.UseRateLimiter();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

// 11. Route Endpoints & SignalR Hubs
app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<VideoCallHub>("/hubs/video-call");

app.Run();
