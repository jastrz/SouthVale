using TownManager.Domain.Entities;
using TownManager.Domain.Entities.Villages;
using TownManager.Domain.Enums;

namespace TownManager.Domain.Config;

public record BuildingEffects
{
    public Resources ProductionPerHour { get; init; } = Resources.Zero;
    public int WarehouseCapacity { get; init; } = 0;
    public int GranaryCapacity { get; init; } = 0;
    public double TrainingSpeedMultiplier { get; init; } = 1.0;

    // Positional convenience constructor
    public BuildingEffects(
        Resources? productionPerHour = null,
        int warehouseCapacity = 0,
        int granaryCapacity = 0,
        double trainingSpeedMultiplier = 1.0)
    {
        ProductionPerHour = productionPerHour ?? Resources.Zero;
        WarehouseCapacity = warehouseCapacity;
        GranaryCapacity = granaryCapacity;
        TrainingSpeedMultiplier = trainingSpeedMultiplier;
    }
}

public record BuildingLevelConfig(
    int Level,
    Resources UpgradeCost,
    TimeSpan UpgradeTime,
    BuildingEffects Effects
);

public static class BuildingConfig
{
    public static readonly Dictionary<BuildingType, List<BuildingLevelConfig>> Levels = new()
    {
        [BuildingType.WoodCutter] =
        [
            new(1, new Resources(40,   100, 50,  60),  TimeSpan.FromSeconds(30),  new BuildingEffects(new Resources(5,  0, 0, 0))),
            new(2, new Resources(80,   200, 100, 120), TimeSpan.FromMinutes(5),   new BuildingEffects(new Resources(9,  0, 0, 0))),
            new(3, new Resources(160,  400, 200, 240), TimeSpan.FromMinutes(15),  new BuildingEffects(new Resources(15, 0, 0, 0))),
            new(4, new Resources(320,  800, 400, 480), TimeSpan.FromMinutes(40),  new BuildingEffects(new Resources(22, 0, 0, 0))),
            new(5, new Resources(640,  1600, 800, 960), TimeSpan.FromHours(2),    new BuildingEffects(new Resources(33, 0, 0, 0))),
        ],

        [BuildingType.ClayPit] =
        [
            new(1, new Resources(80,   40,  50,  60),  TimeSpan.FromSeconds(30),  new BuildingEffects(new Resources(0, 5,  0, 0))),
            new(2, new Resources(160,  80,  100, 120), TimeSpan.FromMinutes(1),   new BuildingEffects(new Resources(0, 9,  0, 0))),
            new(3, new Resources(320,  160, 200, 240), TimeSpan.FromMinutes(15),  new BuildingEffects(new Resources(0, 15, 0, 0))),
            new(4, new Resources(640,  320, 400, 480), TimeSpan.FromMinutes(40),  new BuildingEffects(new Resources(0, 22, 0, 0))),
            new(5, new Resources(1280, 640, 800, 960), TimeSpan.FromHours(2),    new BuildingEffects(new Resources(0, 33, 0, 0))),
        ],

        [BuildingType.IronMine] =
        [
            new(1, new Resources(100,  80,  30,  60),  TimeSpan.FromSeconds(30),  new BuildingEffects(new Resources(0, 0, 5,  0))),
            new(2, new Resources(200,  160, 60,  120), TimeSpan.FromMinutes(5),   new BuildingEffects(new Resources(0, 0, 9,  0))),
            new(3, new Resources(1,  1, 1, 1), TimeSpan.FromSeconds(15),  new BuildingEffects(new Resources(0, 0, 15, 0))),
            new(4, new Resources(800,  640, 240, 480), TimeSpan.FromMinutes(40),  new BuildingEffects(new Resources(0, 0, 22, 0))),
            new(5, new Resources(1600, 1280, 480, 960), TimeSpan.FromHours(2),    new BuildingEffects(new Resources(0, 0, 33, 0))),
        ],

        [BuildingType.CropField] =
        [
            new(1, new Resources(70,   90,  70,  20),  TimeSpan.FromSeconds(30),  new BuildingEffects(new Resources(0, 0, 0, 5))),
            new(2, new Resources(140,  180, 140, 40),  TimeSpan.FromMinutes(5),   new BuildingEffects(new Resources(0, 0, 0, 9))),
            new(3, new Resources(280,  360, 280, 80),  TimeSpan.FromMinutes(15),  new BuildingEffects(new Resources(0, 0, 0, 15))),
            new(4, new Resources(560,  720, 560, 160), TimeSpan.FromMinutes(40),  new BuildingEffects(new Resources(0, 0, 0, 22))),
            new(5, new Resources(1120, 1440, 1120, 320), TimeSpan.FromHours(2),  new BuildingEffects(new Resources(0, 0, 0, 33))),
        ],

        [BuildingType.Warehouse] =
        [
            new(1, new Resources(130,  160, 90,  40),  TimeSpan.FromMinutes(2),  new BuildingEffects(warehouseCapacity: 800)),
            new(2, new Resources(260,  320, 180, 80),  TimeSpan.FromMinutes(8),  new BuildingEffects(warehouseCapacity: 1600)),
            new(3, new Resources(520,  640, 360, 160), TimeSpan.FromMinutes(25), new BuildingEffects(warehouseCapacity: 2800)),
            new(4, new Resources(1040, 1280, 720, 320), TimeSpan.FromHours(1),  new BuildingEffects(warehouseCapacity: 4500)),
            new(5, new Resources(2080, 2560, 1440, 640), TimeSpan.FromHours(3), new BuildingEffects(warehouseCapacity: 7000)),
        ],

        [BuildingType.Granary] =
        [
            new(1, new Resources(80,   100, 70,  20),  TimeSpan.FromMinutes(2),  new BuildingEffects(granaryCapacity: 800)),
            new(2, new Resources(160,  200, 140, 40),  TimeSpan.FromMinutes(8),  new BuildingEffects(granaryCapacity: 1600)),
            new(3, new Resources(320,  400, 280, 80),  TimeSpan.FromMinutes(25), new BuildingEffects(granaryCapacity: 2800)),
            new(4, new Resources(640,  800, 560, 160), TimeSpan.FromHours(1),   new BuildingEffects(granaryCapacity: 4500)),
            new(5, new Resources(1280, 1600, 1120, 320), TimeSpan.FromHours(3), new BuildingEffects(granaryCapacity: 7000)),
        ],

        [BuildingType.Barracks] =
        [
            new(1, new Resources(200,   150, 80,   100), TimeSpan.FromMinutes(5),  new BuildingEffects(trainingSpeedMultiplier: 1.0)),
            new(2, new Resources(400,   300, 160,  200), TimeSpan.FromMinutes(15), new BuildingEffects(trainingSpeedMultiplier: 1.1)),
            new(3, new Resources(800,   600, 320,  400), TimeSpan.FromMinutes(40), new BuildingEffects(trainingSpeedMultiplier: 1.25)),
            new(4, new Resources(1600,  1200, 640,  800), TimeSpan.FromHours(2),  new BuildingEffects(trainingSpeedMultiplier: 1.45)),
            new(5, new Resources(3200,  2400, 1280, 1600), TimeSpan.FromHours(5), new BuildingEffects(trainingSpeedMultiplier: 1.7)),
        ],
    };

    public static BuildingLevelConfig Get(BuildingType type, int level) =>
        Levels[type].First(x => x.Level == level);

    public static BuildingEffects GetEffects(BuildingType type, int level) =>
        Get(type, level).Effects;
    
    public static BuildingEffects AggregateEffects(IEnumerable<Building> buildings) =>
        buildings.Aggregate(
            new BuildingEffects(), 
            (acc, b) =>
            {
                var e = GetEffects(b.Type, b.Level);
                return acc with
                {
                    ProductionPerHour = acc.ProductionPerHour.Add(e.ProductionPerHour),
                    WarehouseCapacity = acc.WarehouseCapacity + e.WarehouseCapacity,
                    GranaryCapacity = acc.GranaryCapacity + e.GranaryCapacity,
                    TrainingSpeedMultiplier = acc.TrainingSpeedMultiplier * e.TrainingSpeedMultiplier
                };
            });
}