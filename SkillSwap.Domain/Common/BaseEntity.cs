namespace SkillSwap.Domain.Common;

/// <summary>
/// Base contract for all domain entities with a strongly typed identifier.
/// </summary>
public abstract class BaseEntity<TId>
{
    public TId Id { get; set; } = default!;
}

/// <summary>
/// Base entity with Guid as primary key.
/// </summary>
public abstract class BaseEntity : BaseEntity<Guid>
{
    protected BaseEntity()
    {
        Id = Guid.NewGuid();
    }
}
