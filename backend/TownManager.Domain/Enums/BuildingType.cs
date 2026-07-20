namespace TownManager.Domain.Enums;

/// <summary>
/// Types of structures that can be constructed in a village (storage, production, military, resource extraction).
/// </summary>
public enum BuildingType
{
    Warehouse = 0,
    [Obsolete] Granary = 1,
    Barracks = 2,
    IronMine = 3,
    WoodCutter = 4,
    Brewery = 5,
    ClayPit = 6,
    Stable = 7,
    Wall = 8,
    Cranny = 9,
    TradePost = 10
}