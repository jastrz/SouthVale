using TownManager.Domain.Enums;

namespace TownManager.Domain.Entities.Villages;

/// <summary>
/// A player-owned settlement; root for its buildings, build/train orders, and garrison.
/// </summary>
public class Village : Entity
{
    public string Name { get; set; } = string.Empty;

    // Current state
    public Resources Resources { get; set; } = Resources.Zero;
    public Troops Troops { get; set; } = Troops.Zero;
    public DateTime LastTickAt { get; set; } = DateTime.UtcNow;

    // Compositions
    public ICollection<Building> Buildings { get; set; } = [];

    // Queues
    public ICollection<BuildOrder> BuildOrders { get; set; } = [];
    public ICollection<TrainOrder> TrainOrders { get; set; } = [];

    // FK
    public Guid PlayerId { get; set; }
    public Player Player { get; set; } = null!;
    
    public static Village CreateStarter(string username) => new()
    {
        Id = Guid.NewGuid(),
        Name = $"{username}'s village",
        Resources = new Resources(500, 500, 500, 500),
        Troops = Troops.Zero,
        LastTickAt = DateTime.UtcNow,
        Buildings =
        [
            Building.Create(BuildingType.ClayPit,  1),
            Building.Create(BuildingType.IronMine,  1),
            Building.Create(BuildingType.Warehouse,  1),
            Building.Create(BuildingType.Granary,  1),
        ]
    };
}