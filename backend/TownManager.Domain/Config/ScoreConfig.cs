using TownManager.Domain.Enums;

namespace TownManager.Domain.Config;

public static class ScoreConfig
{
    public static readonly Dictionary<TroopType, int> Troop = new()
    {
        [TroopType.Swordsman] = 2,
        [TroopType.Archer] = 2,
        [TroopType.Settler] = 0,
    };

    public static readonly Dictionary<BuildingType, IReadOnlyDictionary<int, int>> Building = new()
    {
        [BuildingType.WoodCutter] = Lv(10, 25, 50, 100, 200),
        [BuildingType.ClayPit]    = Lv(10, 25, 50, 100, 200),
        [BuildingType.IronMine]   = Lv(10, 25, 50, 100, 200),
        [BuildingType.CropField]  = Lv(10, 25, 50, 100, 200),
        [BuildingType.Warehouse]  = Lv(10, 25, 50, 100, 200),
        [BuildingType.Granary]    = Lv(10, 25, 50, 100, 200),
        [BuildingType.Barracks]   = Lv(10, 25, 50, 100, 200),
    };

    private static IReadOnlyDictionary<int, int> Lv(params int[] scores) =>
        scores.Select((s, i) => (Level: i + 1, Score: s))
              .ToDictionary(x => x.Level, x => x.Score);
}
