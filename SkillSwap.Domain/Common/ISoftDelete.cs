namespace SkillSwap.Domain.Common;

/// <summary>
/// Marker interface for soft-deletable entities to prevent destructive SQL deletes.
/// </summary>
public interface ISoftDelete
{
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}

/// <summary>
/// Marker interface for enterprise domain events.
/// </summary>
public interface IDomainEvent
{
    public DateTime OccurredOn { get; }
}
