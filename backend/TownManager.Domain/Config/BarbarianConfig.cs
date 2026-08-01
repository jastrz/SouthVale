using TownManager.Domain.Entities;
using TownManager.Domain.Enums;

namespace TownManager.Domain.Config;

public static class BarbarianConfig
{
    // player ID for the barbarian
    public static readonly Guid BarbarianPlayerId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public const int MaxBuildingLevel = 10;
    public const int AttackRange = 25;
    public static int TargetPopulation { get; set; } = 15;
    
    public static double MaxTroopRatio { get; set; } = 0.5;
    public static readonly Troops StartingTroops = new(5, 5, 0, 10, 0, 5);
    public static readonly Troops MaxTroops = new(200, 200, 0, 300, 50, 20);
    public static readonly Resources StartingResources = new(500, 500, 500, 500);
    public static readonly TimeSpan AttackCooldown = TimeSpan.FromHours(2);

    public static readonly Dictionary<BuildingType, int> StartingBuildings = new()
    {
        [BuildingType.ClayPit] = 2,
        [BuildingType.IronMine] = 2,
        [BuildingType.Warehouse] = 2,
        [BuildingType.WoodCutter] = 2,
        [BuildingType.Brewery] = 2,
        [BuildingType.Barracks] = 1,
        [BuildingType.Stable] = 1,
        [BuildingType.TownHall] = 1
    };
}
