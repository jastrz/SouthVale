using TownManager.Domain.Config;
using TownManager.Domain.Enums;

namespace TownManager.Domain.Entities.Villages;

/// <summary>
/// A player-owned settlement; root for its buildings, build/train orders, and garrison.
/// </summary>
public class Village : Entity
{
    public string Name { get; set; } = string.Empty;
    public VillageType VillageType { get; set; }

    // Current state
    public Resources Resources { get; set; } = Resources.Zero;
    public Troops Troops { get; set; } = Troops.Zero;
    public DateTime LastTickAt { get; set; } = DateTime.UtcNow;

    // Map
    public Coordinates Coordinates { get; init; } = new(0, 0);

    // Compositions
    public ICollection<Building> Buildings { get; set; } = [];

    // Queues
    public ICollection<BuildOrder> BuildOrders { get; set; } = [];
    public ICollection<TrainOrder> TrainOrders { get; set; } = [];
    public ICollection<TroopMovement> TroopMovements { get; set; } = [];

    // FK
    public Guid PlayerId { get; set; }
    public Player Player { get; set; } = null!;


    public DateTime? LastAttackAt { get; set; }

    public static Village CreateStarter(string villageName, Coordinates coordinates) => new()
    {
        Id = Guid.NewGuid(),
        Name = villageName,
        VillageType = VillageType.Player,
        Resources = new Resources(500, 500, 500, 500),
        Troops = Troops.Zero,
        LastTickAt = DateTime.UtcNow,
        Buildings =
        [
            Building.Create(BuildingType.ClayPit,  1),
            Building.Create(BuildingType.IronMine,  1),
            Building.Create(BuildingType.Warehouse,  1),
            Building.Create(BuildingType.WoodCutter, 1),
            Building.Create(BuildingType.Brewery, 1)
        ],
        Coordinates = coordinates
    };

    public void ApplyProduction(BuildingEffects effects)
    {
        var elapsed = DateTime.UtcNow - LastTickAt;
        Resources = Cap(Resources.Add(effects.ProductionPerHour.Multiply(elapsed.TotalHours)), effects);
        LastTickAt = DateTime.UtcNow;
    }

    public Resources GetCurrentResources(BuildingEffects effects)
    {
        var elapsed = DateTime.UtcNow - LastTickAt;
        return Cap(Resources.Add(effects.ProductionPerHour.Multiply(elapsed.TotalHours)), effects);
    }

private static Resources Cap(Resources r, BuildingEffects e) => new(
    Math.Min(r.Wood, e.WarehouseCapacity),
    Math.Min(r.Clay, e.WarehouseCapacity),
    Math.Min(r.Iron, e.WarehouseCapacity),
    Math.Min(r.Beer, e.WarehouseCapacity)
);
}
