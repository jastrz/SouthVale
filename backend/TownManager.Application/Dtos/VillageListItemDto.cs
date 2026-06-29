using TownManager.Domain.Entities;

namespace TownManager.Application.Dtos;

public record VillageListItemDto(
    Guid Id,
    string Name,
    ResourcesDto Resources,
    TroopsDto Troops,
    Coordinates Coordinates
);
