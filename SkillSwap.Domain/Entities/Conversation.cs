using SkillSwap.Domain.Common;

namespace SkillSwap.Domain.Entities;

/// <summary>
/// Direct 1-on-1 chat conversation between two users.
/// </summary>
public class Conversation : BaseAuditableEntity, ISoftDelete
{
    public Guid UserOneId { get; set; }
    public Guid UserTwoId { get; set; }
    public Guid? SwapRequestId { get; set; }
    public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation
    public virtual ApplicationUser UserOne { get; set; } = null!;
    public virtual ApplicationUser UserTwo { get; set; } = null!;
    public virtual SwapRequest? SwapRequest { get; set; }
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}
