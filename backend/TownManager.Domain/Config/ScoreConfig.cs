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
        [BuildingType.WoodCutter] = Lv(10,20,40,80,150,250,400,600,900,1300,1800,2500,3500,5000,7000,10000,14000,20000,28000,40000),
        [BuildingType.ClayPit]    = Lv(10,20,40,80,150,250,400,600,900,1300,1800,2500,3500,5000,7000,10000,14000,20000,28000,40000),
        [BuildingType.IronMine]   = Lv(10,20,40,80,150,250,400,600,900,1300,1800,2500,3500,5000,7000,10000,14000,20000,28000,40000),
        [BuildingType.Brewery]    = Lv(10,20,40,80,150,250,400,600,900,1300,1800,2500,3500,5000,7000,10000,14000,20000,28000,40000),
        [BuildingType.Warehouse]  = Lv(10,20,40,80,150,250,400,600,900,1300,1800,2500,3500,5000,7000,10000,14000,20000,28000,40000),
        [BuildingType.Barracks]   = Lv(10,20,40,80,150,250,400,600,900,1300,1800,2500,3500,5000,7000,10000,14000,20000,28000,40000),
        [BuildingType.Stable]     = Lv(10,20,40,80,150,250,400,600,900,1300,1800,2500,3500,5000,7000,10000,14000,20000,28000,40000),
        [BuildingType.Wall]       = Lv(10,20,40,80,150,250,400,600,900,1300,1800,2500,3500,5000,7000,10000,14000,20000,28000,40000),
        [BuildingType.Cranny]     = Lv(10,20,40,80,150,250,400,600,900,1300,1800,2500,3500,5000,7000,10000,14000,20000,28000,40000),
        [BuildingType.TradePost]  = Lv(10,20,40,80,150,250,400,600,900,1300),
    };

    private static IReadOnlyDictionary<int, int> Lv(params int[] scores) =>
        scores.Select((s, i) => (Level: i + 1, Score: s))
              .ToDictionary(x => x.Level, x => x.Score);
}
