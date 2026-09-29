using Microsoft.AspNetCore.Identity;

namespace SkillSwap.Domain.Entities;

/// <summary>
/// Core application user entity integrating ASP.NET Core Identity with platform profile attributes.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public ApplicationUser()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        SecurityStamp = Guid.NewGuid().ToString();
    }

    public string FullName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Location { get; set; }
    public string? TimeZone { get; set; }
    public bool IsActive { get; set; } = true;

    // Platform statistics & reputation
    public double AverageRating { get; set; } = 0.0;
    public int TotalReviewsCount { get; set; } = 0;
    public int TotalSwapsCompleted { get; set; } = 0;

    // Security & Two-Factor Authentication (2FA)
    public string? TwoFactorSecretKey { get; set; }
    public string? TwoFactorRecoveryCodes { get; set; }

    // External Social Logins (Google, Apple, Facebook)
    public string? GoogleId { get; set; }
    public string? AppleId { get; set; }
    public string? FacebookId { get; set; }

    // Stripe Customer ID
    public string? StripeCustomerId { get; set; }

    // Refresh Token Management
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation Properties
    public virtual ICollection<UserSkill> SkillsOffered { get; set; } = new List<UserSkill>();
    public virtual ICollection<UserDesiredSkill> SkillsWanted { get; set; } = new List<UserDesiredSkill>();
    public virtual ICollection<UserAvailability> Availabilities { get; set; } = new List<UserAvailability>();
    public virtual ICollection<SwapRequest> SentSwapRequests { get; set; } = new List<SwapRequest>();
    public virtual ICollection<SwapRequest> ReceivedSwapRequests { get; set; } = new List<SwapRequest>();
    public virtual ICollection<SwapSession> HostedSessions { get; set; } = new List<SwapSession>();
    public virtual ICollection<SwapSession> ParticipatedSessions { get; set; } = new List<SwapSession>();
    public virtual ICollection<Review> ReviewsGiven { get; set; } = new List<Review>();
    public virtual ICollection<Review> ReviewsReceived { get; set; } = new List<Review>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public virtual ICollection<Report> ReportsFiled { get; set; } = new List<Report>();
    public virtual ICollection<Report> ReportsReceived { get; set; } = new List<Report>();
    public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
    public virtual ICollection<UserSubscription> Subscriptions { get; set; } = new List<UserSubscription>();
}
