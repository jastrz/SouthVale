namespace TownManager.Domain.Enums;

/// <summary>
/// Types of structures that can be constructed in a village (storage, production, military, resource extraction).
/// </summary>
public enum BuildingType
{
    Warehouse = 0,
    Granary = 1,
    Barracks = 2,
    IronMine = 3,
    WoodCutter = 4,
    CropField = 5,
    ClayPit = 6
}