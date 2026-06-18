using TownManager.Domain.Entities;
using TownManager.Domain.Enums;

namespace TownManager.Application.Dtos;

public record MovementDto(
    Guid Id,
    MovementType Type,
    MovementStatus Status,
    DateTime DepartureAt,
    DateTime ArrivesAt,
    DateTime? CompletedAt,
    TroopsDto Troops,
    Guid OriginVillageId,
    string OriginVillageName,
    Guid? TargetVillageId,
    string? TargetVillageName,
    Coordinates? TargetCoordinates
);
