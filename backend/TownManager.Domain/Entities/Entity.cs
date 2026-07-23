using System.ComponentModel.DataAnnotations.Schema;
using MediatR;

namespace TownManager.Domain.Entities;

/// <summary>
/// Base entity with an identifier and audit timestamps; generic key allows non-Guid IDs in subclasses.
/// </summary>
public abstract class Entity<TId>
{
    private List<INotification> _events = [];

    [NotMapped]
    public IReadOnlyCollection<INotification> Events => _events.AsReadOnly();

    public TId Id { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public void AddDomainEvent(INotification eventItem) => _events.Add(eventItem);
    public void RemoveDomainEvent(INotification eventItem) => _events.Remove(eventItem);
    public void ClearDomainEvents() => _events.Clear();
}

/// <summary>
/// Default entity base using Guid as the primary key.
/// </summary>
public abstract class Entity : Entity<Guid> { }