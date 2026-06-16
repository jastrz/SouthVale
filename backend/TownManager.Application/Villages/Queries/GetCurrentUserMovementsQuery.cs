using MediatR;
using TownManager.Application.Common;
using TownManager.Domain.Enums;

namespace TownManager.Application.Villages.Queries;

public record GetCurrentUserMovementsQuery(string UserId)
    : IRequest<Result<IReadOnlyList<MovementDto>>>;

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
    int? TargetMapX,
    int? TargetMapY
);
