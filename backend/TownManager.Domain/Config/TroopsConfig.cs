using TownManager.Domain.Entities;
using TownManager.Domain.Enums;

namespace TownManager.Domain.Config;

public record TroopStats
{
    public int Attack { get; init; }
    public int Defense { get; init; }
    public int CarryCapacity { get; init; }
    public int Speed { get; init; }

    public TroopStats(int attack, int defense, int carryCapacity, int speed)
    {
        Attack = attack; Defense = defense; CarryCapacity = carryCapacity; Speed = speed;
    }
}

public record TroopConfig(
    TroopType Type,
    Resources TrainingCost,
    TimeSpan TrainingTime,
    TroopStats Stats,
    BuildingType TrainedAt,
    int Score
);

public static class TroopsConfig
{
    private static TimeSpan T(string s) => s switch
    {
        not null when s.EndsWith('d') => TimeSpan.FromDays(double.Parse(s[..^1])),
        not null when s.EndsWith('h') => TimeSpan.FromHours(double.Parse(s[..^1])),
        not null when s.EndsWith('m') => TimeSpan.FromMinutes(double.Parse(s[..^1])),
        not null when s.EndsWith('s') => TimeSpan.FromSeconds(double.Parse(s[..^1])),
        _ => throw new FormatException($"Unknown duration: {s}")
    };

    // csv/troops.csv columns:
    //   Type,Attack,Defense,CarryCapacity,Speed,
    //   CostWood,CostClay,CostIron,CostBrewery,TrainingTime,TrainedAt,Score
    private static Dictionary<TroopType, TroopConfig> LoadFromCsv()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "data", "troops.csv");
        var lines = File.ReadAllLines(path);
        var result = new Dictionary<TroopType, TroopConfig>();

        foreach (var line in lines.Skip(1))
        {
            var parts = line.Split(',');
            if (!Enum.TryParse<TroopType>(parts[0], out var type)) continue;
            if (!Enum.TryParse<BuildingType>(parts[10], out var trainedAt)) continue;

            result[type] = new(
                type,
                new Resources(I(parts[5]), I(parts[6]), I(parts[7]), I(parts[8])),
                T(parts[9]),
                new TroopStats(I(parts[1]), I(parts[2]), I(parts[3]), I(parts[4])),
                trainedAt,
                I(parts[11])
            );
        }

        return result;

        static int I(string s) => string.IsNullOrEmpty(s) ? 0 : int.Parse(s);
    }

    public static readonly Dictionary<TroopType, TroopConfig> All = LoadFromCsv();

    public static TroopConfig Get(TroopType type) => All[type];

    public static TimeSpan CalculateTrainingTime(TroopType type, int count, double speedMultiplier = 1.0) =>
        All[type].TrainingTime * count / speedMultiplier;

    public static int GetSlowestSpeed(Troops troops)
    {
        if (troops.IsEmpty()) return 0;
        return Enum.GetValues<TroopType>()
            .Select(t => (type: t, count: troops.Get(t)))
            .Where(x => x.count > 0)
            .Select(x => All[x.type].Stats.Speed)
            .Min();
    }
}
