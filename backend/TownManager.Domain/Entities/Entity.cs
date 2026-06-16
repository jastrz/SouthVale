namespace TownManager.Domain.Entities;

/// <summary>
/// Base entity with an identifier and audit timestamps; generic key allows non-Guid IDs in subclasses.
/// </summary>
public abstract class Entity<TId>
{
    public TId Id { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Default entity base using Guid as the primary key.
/// </summary>
public abstract class Entity : Entity<Guid> { }