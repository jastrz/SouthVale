using TownManager.Domain.Entities.Villages;

namespace TownManager.Domain.Entities;

/// <summary>
/// A user account; owns one or more villages and has a currently active one.
/// </summary>
public class Player : Entity
{
    public required string Username { get; init; }
    public Guid? CurrentVillageId { get; set; }
    public ICollection<Village> Villages { get; set; } = [];
    
    public static Player Create(string username, Village starterVillage) => new()
    {
        Id = Guid.NewGuid(),
        Username = username,
        Villages = [starterVillage]
    };

}