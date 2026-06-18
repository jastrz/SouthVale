using TownManager.Domain.Enums;

namespace TownManager.Application.Dtos;

public record BuildOrderDto(
    Guid Id,
    BuildingType BuildingType,
    int TargetLevel,
    DateTime StartsAt,
    DateTime CompletesAt
);

