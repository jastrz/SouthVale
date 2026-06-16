using TownManager.Domain.Enums;

namespace TownManager.Domain.Entities.Villages;

/// <summary>
/// A constructed structure of a given type in a village, with its current upgrade level.
/// </summary>
public class Building : Entity
{
    public BuildingType Type { get; set; }
    public int Level { get; set; } = 1;

    public Guid VillageId { get; set; }

    public static Building Create(BuildingType buildingType, int level)
    {
        return new Building
        {
            Type = buildingType,
            Level = level,
        };
    }
}