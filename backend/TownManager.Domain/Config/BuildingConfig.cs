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
    public double DefenseMultiplier { get; init; } = 1.0;
    public int CrannyCapacity { get; init; } = 0;
    public double TradeRate { get; init; } = 1.0;

    public BuildingEffects(
        Resources? productionPerHour = null,
        int warehouseCapacity = 0,
        int granaryCapacity = 0,
        double trainingSpeedMultiplier = 1.0,
        double defenseMultiplier = 1.0,
        int crannyCapacity = 0,
        double tradeRate = 1.0)
    {
        ProductionPerHour = productionPerHour ?? Resources.Zero;
        WarehouseCapacity = warehouseCapacity;
        GranaryCapacity = granaryCapacity;
        TrainingSpeedMultiplier = trainingSpeedMultiplier;
        DefenseMultiplier = defenseMultiplier;
        CrannyCapacity = crannyCapacity;
        TradeRate = tradeRate;
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
    private static TimeSpan T(string s) => s switch
    {
        not null when s.EndsWith('d') => TimeSpan.FromDays(double.Parse(s[..^1])),
        not null when s.EndsWith('h') => TimeSpan.FromHours(double.Parse(s[..^1])),
        not null when s.EndsWith('m') => TimeSpan.FromMinutes(double.Parse(s[..^1])),
        not null when s.EndsWith('s') => TimeSpan.FromSeconds(double.Parse(s[..^1])),
        _ => throw new FormatException($"Unknown duration: {s}")
    };

    // parses csv/buildings.csv into Levels.
    // I = parse int (empty → 0), M = parse double (empty → 1, for multipliers)
    // csv columns: Building,Level, CostW,CostC,CostI,CostB, Time, ProdW,ProdC,ProdB,ProdBe, WhCap,GrCap,TrainM,DefM,CrCap,Rate,Score
    //                         [0]   [1]   [2]   [3]   [4]   [5]  [6]   [7]   [8]   [9]  [10]  [11]  [12]  [13]  [14] [15]  [16]  [17]
    private static Dictionary<BuildingType, List<BuildingLevelConfig>> LoadFromCsv()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "csv", "buildings.csv");
        if (!File.Exists(path)) return [];
        var lines = File.ReadAllLines(path);
        var result = new Dictionary<BuildingType, List<BuildingLevelConfig>>();
        var currentType = (BuildingType)(-1);
        List<BuildingLevelConfig>? currentList = null;

        static int I(string s) => string.IsNullOrEmpty(s) ? 0 : int.Parse(s);
        static double M(string s) => string.IsNullOrEmpty(s) ? 1 : double.Parse(s); // multipliers default 1

        foreach (var line in lines.Skip(1))
        {
            var parts = line.Split(',');
            var typeName = parts[0];
            if (!Enum.TryParse<BuildingType>(typeName, out var type))
                continue;

            if (type != currentType)
            {
                currentType = type;
                currentList = [];
                result[type] = currentList;
            }

            var level = I(parts[1]);
            var cost = new Resources(I(parts[2]), I(parts[3]), I(parts[4]), I(parts[5]));
            var upgradeTime = T(parts[6]);
            var prod = new Resources(I(parts[7]), I(parts[8]), I(parts[9]), I(parts[10]));
            var effects = new BuildingEffects(
                prod,
                warehouseCapacity: I(parts[11]),
                granaryCapacity: I(parts[12]),
                trainingSpeedMultiplier: M(parts[13]),
                defenseMultiplier: M(parts[14]),
                crannyCapacity: I(parts[15]),
                tradeRate: M(parts[16]));

            currentList!.Add(new BuildingLevelConfig(level, cost, upgradeTime, effects));
        }

        return result;
    }

    public static readonly Dictionary<BuildingType, List<BuildingLevelConfig>> Levels = LoadFromCsv();

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
                    TrainingSpeedMultiplier = acc.TrainingSpeedMultiplier * e.TrainingSpeedMultiplier,
                    DefenseMultiplier = acc.DefenseMultiplier * e.DefenseMultiplier,
                    CrannyCapacity = acc.CrannyCapacity + e.CrannyCapacity,
                    TradeRate = acc.TradeRate * e.TradeRate,
                };
            });
}