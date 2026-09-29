using SkillSwap.Domain.Common;

namespace SkillSwap.Domain.Entities;

/// <summary>
/// Peer evaluation and rating submitted after a swap session concludes.
/// </summary>
public class Review : BaseAuditableEntity, ISoftDelete
{
    public Guid SessionId { get; set; }
    public Guid ReviewerId { get; set; }
    public Guid RevieweeId { get; set; }

    public int OverallRating { get; set; } // 1 - 5
    public int PunctualityScore { get; set; } // 1 - 5
    public int CommunicationScore { get; set; } // 1 - 5
    public int KnowledgeScore { get; set; } // 1 - 5

    public string? Comment { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation
    public virtual SwapSession Session { get; set; } = null!;
    public virtual ApplicationUser Reviewer { get; set; } = null!;
    public virtual ApplicationUser Reviewee { get; set; } = null!;
}
