using TownManager.Domain.Entities;
using TownManager.Domain.Enums;

namespace TownManager.Domain.Config;

public static class BarbarianConfig
{
    // player ID for the barbarian
    public static readonly Guid BarbarianPlayerId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public const int MaxBuildingLevel = 5;
    public const int AttackRange = 15;
    public const int TargetPopulation = 15;
    
    public static readonly Troops StartingTroops = new(15, 15, 0);
    public static readonly Troops MaxTroops = new(200, 200, 0);
    public static readonly Resources StartingResources = new(500, 500, 500, 500);
    public static readonly TimeSpan AttackCooldown = TimeSpan.FromHours(2);

    public static readonly Dictionary<BuildingType, int> StartingBuildings = new()
    {
        [BuildingType.ClayPit] = 2,
        [BuildingType.IronMine] = 2,
        [BuildingType.Warehouse] = 2,
        [BuildingType.Granary] = 2,
        [BuildingType.WoodCutter] = 2,
        [BuildingType.CropField] = 2,
    };
}
