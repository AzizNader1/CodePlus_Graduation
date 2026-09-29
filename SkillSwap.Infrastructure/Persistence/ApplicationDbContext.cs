using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Domain.Common;
using SkillSwap.Domain.Entities;

namespace SkillSwap.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<UserSkill> UserSkills => Set<UserSkill>();
    public DbSet<UserDesiredSkill> UserDesiredSkills => Set<UserDesiredSkill>();
    public DbSet<UserAvailability> UserAvailabilities => Set<UserAvailability>();
    public DbSet<SwapRequest> SwapRequests => Set<SwapRequest>();
    public DbSet<SwapSession> SwapSessions => Set<SwapSession>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
    public DbSet<UserSubscription> UserSubscriptions => Set<UserSubscription>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Customize Identity Table Names
        builder.Entity<ApplicationUser>(b => b.ToTable("Users"));
        builder.Entity<ApplicationRole>(b => b.ToTable("Roles"));

        // Global query filter for soft-deleted entities
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                builder.Entity(entityType.ClrType)
                    .HasQueryFilter(ConvertFilterExpression(entityType.ClrType));
            }
        }

        // Configure Category & Skill
        builder.Entity<Category>(b =>
        {
            b.HasKey(c => c.Id);
            b.Property(c => c.Name).IsRequired().HasMaxLength(100);
            b.HasMany(c => c.Skills).WithOne(s => s.Category).HasForeignKey(s => s.CategoryId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Skill>(b =>
        {
            b.HasKey(s => s.Id);
            b.Property(s => s.Name).IsRequired().HasMaxLength(150);
        });

        // UserSkill (Offered)
        builder.Entity<UserSkill>(b =>
        {
            b.HasKey(us => us.Id);
            b.HasOne(us => us.User).WithMany(u => u.SkillsOffered).HasForeignKey(us => us.UserId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(us => us.Skill).WithMany(s => s.UserSkills).HasForeignKey(us => us.SkillId).OnDelete(DeleteBehavior.Restrict);
        });

        // UserDesiredSkill (Wanted)
        builder.Entity<UserDesiredSkill>(b =>
        {
            b.HasKey(ds => ds.Id);
            b.HasOne(ds => ds.User).WithMany(u => u.SkillsWanted).HasForeignKey(ds => ds.UserId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(ds => ds.Skill).WithMany(s => s.DesiredByUsers).HasForeignKey(ds => ds.SkillId).OnDelete(DeleteBehavior.Restrict);
        });

        // UserAvailability
        builder.Entity<UserAvailability>(b =>
        {
            b.HasKey(ua => ua.Id);
            b.HasOne(ua => ua.User).WithMany(u => u.Availabilities).HasForeignKey(ua => ua.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        // SwapRequest
        builder.Entity<SwapRequest>(b =>
        {
            b.HasKey(sr => sr.Id);
            b.HasOne(sr => sr.Requester).WithMany(u => u.SentSwapRequests).HasForeignKey(sr => sr.RequesterId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(sr => sr.Receiver).WithMany(u => u.ReceivedSwapRequests).HasForeignKey(sr => sr.ReceiverId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(sr => sr.OfferedSkill).WithMany().HasForeignKey(sr => sr.OfferedSkillId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(sr => sr.RequestedSkill).WithMany().HasForeignKey(sr => sr.RequestedSkillId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(sr => sr.Session).WithOne(ss => ss.SwapRequest).HasForeignKey<SwapSession>(ss => ss.SwapRequestId).OnDelete(DeleteBehavior.Cascade);
        });

        // SwapSession
        builder.Entity<SwapSession>(b =>
        {
            b.HasKey(ss => ss.Id);
            b.HasOne(ss => ss.HostUser).WithMany(u => u.HostedSessions).HasForeignKey(ss => ss.HostUserId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(ss => ss.ParticipantUser).WithMany(u => u.ParticipatedSessions).HasForeignKey(ss => ss.ParticipantUserId).OnDelete(DeleteBehavior.Restrict);
        });

        // Conversation & Messages
        builder.Entity<Conversation>(b =>
        {
            b.HasKey(c => c.Id);
            b.HasOne(c => c.UserOne).WithMany().HasForeignKey(c => c.UserOneId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(c => c.UserTwo).WithMany().HasForeignKey(c => c.UserTwoId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(c => c.SwapRequest).WithMany().HasForeignKey(c => c.SwapRequestId).OnDelete(DeleteBehavior.SetNull);
            b.HasMany(c => c.Messages).WithOne(m => m.Conversation).HasForeignKey(m => m.ConversationId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Message>(b =>
        {
            b.HasKey(m => m.Id);
            b.HasOne(m => m.Sender).WithMany().HasForeignKey(m => m.SenderId).OnDelete(DeleteBehavior.Restrict);
        });

        // Reviews
        builder.Entity<Review>(b =>
        {
            b.HasKey(r => r.Id);
            b.HasOne(r => r.Session).WithMany(s => s.Reviews).HasForeignKey(r => r.SessionId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(r => r.Reviewer).WithMany(u => u.ReviewsGiven).HasForeignKey(r => r.ReviewerId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(r => r.Reviewee).WithMany(u => u.ReviewsReceived).HasForeignKey(r => r.RevieweeId).OnDelete(DeleteBehavior.Restrict);
        });

        // Notifications
        builder.Entity<Notification>(b =>
        {
            b.HasKey(n => n.Id);
            b.HasOne(n => n.User).WithMany(u => u.Notifications).HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        // Reports
        builder.Entity<Report>(b =>
        {
            b.HasKey(rp => rp.Id);
            b.HasOne(rp => rp.Reporter).WithMany(u => u.ReportsFiled).HasForeignKey(rp => rp.ReporterId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(rp => rp.ReportedUser).WithMany(u => u.ReportsReceived).HasForeignKey(rp => rp.ReportedUserId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(rp => rp.Session).WithMany().HasForeignKey(rp => rp.SessionId).OnDelete(DeleteBehavior.SetNull);
        });

        // PaymentTransactions
        builder.Entity<PaymentTransaction>(b =>
        {
            b.HasKey(p => p.Id);
            b.Property(p => p.Amount).HasPrecision(18, 2);
            b.HasOne(p => p.User).WithMany(u => u.PaymentTransactions).HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        // UserSubscription
        builder.Entity<UserSubscription>(b =>
        {
            b.HasKey(us => us.Id);
            b.HasOne(us => us.User).WithMany(u => u.Subscriptions).HasForeignKey(us => us.UserId).OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static System.Linq.Expressions.LambdaExpression ConvertFilterExpression(Type entityType)
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(entityType, "e");
        var property = System.Linq.Expressions.Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
        var comparison = System.Linq.Expressions.Expression.Equal(property, System.Linq.Expressions.Expression.Constant(false));
        return System.Linq.Expressions.Expression.Lambda(comparison, parameter);
    }
}
