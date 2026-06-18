using TownManager.Application.Villages.Queries;
using TownManager.Domain.Entities;

namespace TownManager.Application.Dtos;

public record VillageDto(
    Guid Id,
    string Name,
    ResourcesDto Resources,
    TroopsDto Troops,
    IReadOnlyList<BuildingDto> Buildings,
    IReadOnlyList<BuildOrderDto> BuildOrders,
    IReadOnlyList<TrainOrderDto> TrainOrders,
    Coordinates Coordinates
);