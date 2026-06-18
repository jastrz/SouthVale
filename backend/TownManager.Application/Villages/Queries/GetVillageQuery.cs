using MediatR;
using TownManager.Application.Common;
using TownManager.Domain.Entities;
using TownManager.Domain.Enums;

namespace TownManager.Application.Villages.Queries;

public record GetVillageQuery(Guid VillageId) : IRequest<Result<VillageDto>>;

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

public record ResourcesDto(int Wood, int Clay, int Iron, int Crop);
public record TroopsDto(int Swordsmen, int Archers, int Settlers);
public record BuildingDto(Guid Id, string Type, int Level);

public record BuildOrderDto(
    Guid Id,
    BuildingType BuildingType,
    int TargetLevel,
    DateTime StartsAt,
    DateTime CompletesAt
);

public record TrainOrderDto(
    Guid Id,
    TroopType TroopType,
    int Amount,
    int Completed,
    DateTime StartedAt,
    DateTime CompletesAt
);
