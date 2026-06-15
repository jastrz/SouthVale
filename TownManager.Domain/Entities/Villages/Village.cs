using TownManager.Domain.Config;
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
    
    // Map
    public int MapX { get; init; }
    public int MapY { get; init; }

    // Compositions
    public ICollection<Building> Buildings { get; set; } = [];

    // Queues
    public ICollection<BuildOrder> BuildOrders { get; set; } = [];
    public ICollection<TrainOrder> TrainOrders { get; set; } = [];
    public ICollection<TroopMovement> TroopMovements { get; set; } = [];

    // FK
    public Guid PlayerId { get; set; }
    public Player Player { get; set; } = null!;
    
    public static Village CreateStarter(string villageName, (int x, int y) mapCoords) => new()
    {
        Id = Guid.NewGuid(),
        Name = villageName,
        Resources = new Resources(500, 500, 500, 500),
        Troops = Troops.Zero,
        LastTickAt = DateTime.UtcNow,
        Buildings =
        [
            Building.Create(BuildingType.ClayPit,  1),
            Building.Create(BuildingType.IronMine,  1),
            Building.Create(BuildingType.Warehouse,  1),
            Building.Create(BuildingType.Granary,  1),
            Building.Create(BuildingType.WoodCutter, 1),
            Building.Create(BuildingType.CropField, 1)
        ],
        MapX = mapCoords.x,
        MapY = mapCoords.y
    };
    
    public void ApplyProduction(BuildingEffects effects)
    {
        var elapsed = DateTime.UtcNow - LastTickAt;
        Resources = Resources.Add(effects.ProductionPerHour.Multiply(elapsed.TotalHours));
        LastTickAt = DateTime.UtcNow;
    }
}