namespace TownManager.Application.Dtos;

public record VillageStatusDto(
    Guid VillageId,
    int BuildOrderCount,
    int TrainOrderCount,
    ResourcesDto Resources
);
