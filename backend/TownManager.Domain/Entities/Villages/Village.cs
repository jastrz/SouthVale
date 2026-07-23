using TownManager.Domain.Config;
using TownManager.Domain.Enums;
using TownManager.Domain.Events;

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
        Resources = GameSettings.StartingResources,
        Troops = Troops.Zero,
        LastTickAt = DateTime.UtcNow,
        Buildings =
        [
            Building.Create(BuildingType.TownHall, 1),
            Building.Create(BuildingType.ClayPit,  1),
            Building.Create(BuildingType.IronMine,  1),
            Building.Create(BuildingType.Warehouse,  1),
            Building.Create(BuildingType.WoodCutter, 1),
            Building.Create(BuildingType.Brewery, 1)
        ],
        Coordinates = coordinates
    };

    public void Tick(BuildingEffects effects)
    {
        var elapsed = DateTime.UtcNow - LastTickAt;
        var produced = effects.ProductionPerHour.Multiply(elapsed.TotalHours);

        AddProduction(produced);
        var deficit = ApplyUpkeep(elapsed);
        if (deficit > 0)
            ResolveStarvation(deficit, elapsed);

        Resources = Cap(Resources, effects);
        LastTickAt = DateTime.UtcNow;
    }

    public Resources GetCurrentResources(BuildingEffects effects)
    {
        var elapsed = DateTime.UtcNow - LastTickAt;
        var produced = effects.ProductionPerHour.Multiply(elapsed.TotalHours);
        var upkeep = Troops.GetUpkeepPerHour() * elapsed.TotalHours;
        return Cap(Resources.Add(produced).Subtract(new Resources(0, 0, 0, upkeep)), effects);
    }

    private void AddProduction(Resources produced)
    {
        Resources = Resources.Add(produced);
    }

    private double ApplyUpkeep(TimeSpan elapsed)
    {
        var upkeep = Troops.GetUpkeepPerHour() * elapsed.TotalHours;
        Resources = Resources.Subtract(new Resources(0, 0, 0, upkeep));
        return Math.Max(0, -Resources.Beer);
    }

    private void ResolveStarvation(double deficit, TimeSpan elapsed)
    {
        var (remaining, starved) = CalculateStarvation(deficit, Troops, elapsed);
        Troops = remaining;
        if (!starved.IsEmpty())
            AddDomainEvent(new TroopsStarvedEvent(PlayerId, Name, starved));
    }

    internal static (Troops remaining, Troops starved) CalculateStarvation(
        double deficit, Troops troops, TimeSpan elapsed)
    {
        var starved = Troops.Zero;

        var typesOrderedByUpkeep = TroopsConfig.All
            .Select(x => x.Value)
            .Where(x => x.Upkeep > 0)
            .OrderByDescending(x => x.Upkeep)
            .ToList();

        foreach(var troopConfig in typesOrderedByUpkeep)
        {
            var count = troops.Get(troopConfig.Type); 
            if(count <= 0) continue;
                
            var upkeepPerTroop = troopConfig.Upkeep * elapsed.TotalHours;

            if(upkeepPerTroop <= 0) continue;

            var maxLosable = (int)Math.Min(count, Math.Ceiling(deficit / upkeepPerTroop));

            if(maxLosable <= 0) continue;

            troops = troops.Subtract(new Troops { Counts = new() { [troopConfig.Type] = maxLosable } });
            starved = starved.Add(troopConfig.Type, maxLosable);

            deficit -= maxLosable * upkeepPerTroop;

            if(deficit <= 0) break;
        }

        return (troops, starved);
    }

    private static Resources Cap(Resources r, BuildingEffects e) => new(
        Math.Max(0, Math.Min(r.Wood, e.WarehouseCapacity)),
        Math.Max(0, Math.Min(r.Clay, e.WarehouseCapacity)),
        Math.Max(0, Math.Min(r.Iron, e.WarehouseCapacity)),
        Math.Max(0, Math.Min(r.Beer, e.WarehouseCapacity))
    );
}
