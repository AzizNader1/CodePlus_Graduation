using Microsoft.EntityFrameworkCore;
using SkillSwap.Domain.Entities;

namespace SkillSwap.Application.Common.Interfaces;

/// <summary>
/// Abstraction for database operations decoupling Application features from EF Core DbContext implementations.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<ApplicationUser> Users { get; }
    DbSet<Category> Categories { get; }
    DbSet<Skill> Skills { get; }
    DbSet<UserSkill> UserSkills { get; }
    DbSet<UserDesiredSkill> UserDesiredSkills { get; }
    DbSet<UserAvailability> UserAvailabilities { get; }
    DbSet<SwapRequest> SwapRequests { get; }
    DbSet<SwapSession> SwapSessions { get; }
    DbSet<Conversation> Conversations { get; }
    DbSet<Message> Messages { get; }
    DbSet<Review> Reviews { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<Report> Reports { get; }
    DbSet<PaymentTransaction> PaymentTransactions { get; }
    DbSet<UserSubscription> UserSubscriptions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
