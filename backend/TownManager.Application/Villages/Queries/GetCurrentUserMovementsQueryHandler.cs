using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Dtos;
using TownManager.Application.Interfaces;

namespace TownManager.Application.Villages.Queries;

public class GetCurrentUserMovementsQueryHandler(
    IPlayerRepository playerRepo,
    IMovementRepository movementRepo,
    IVillageRepository villageRepo)
    : IRequestHandler<GetCurrentUserMovementsQuery, Result<IReadOnlyList<MovementDto>>>
{
    public async Task<Result<IReadOnlyList<MovementDto>>> Handle(
        GetCurrentUserMovementsQuery q, CancellationToken ct)
    {
        var player = await playerRepo.GetByUserIdAsync(q.UserId, ct);
        if (player is null)
            return Result<IReadOnlyList<MovementDto>>.Failure(["Player not found."], statusCode: 404);

        var villages = await villageRepo.GetSummariesByPlayerAsync(player.Id, ct);
        var villageIds = villages.Select(v => v.Id).ToList();

        var movements = await movementRepo.GetInFlightForPlayerAsync(player.Id, villageIds, ct);

        var targetIds = movements
            .Where(m => m.TargetVillageId.HasValue)
            .Select(m => m.TargetVillageId!.Value)
            .Distinct()
            .ToList();

        var targetNames = targetIds.Count > 0
            ? await villageRepo.GetNamesByIdsAsync(targetIds, ct)
            : new Dictionary<Guid, string>();

        var dtos = movements.Select(m => new MovementDto(
            m.Id,
            m.Type,
            m.Status,
            m.DepartureAt,
            m.ArrivesAt,
            m.CompletedAt,
            new TroopsDto(m.Troops.Swordsmen, m.Troops.Archers, m.Troops.Settlers),
            m.CarriedResources is { } r
                ? new ResourcesDto((int)r.Wood, (int)r.Clay, (int)r.Iron, (int)r.Beer)
                : null,
            m.VillageId,
            m.Village.Name,
            m.TargetVillageId,
            m.TargetVillageId.HasValue ? targetNames.GetValueOrDefault(m.TargetVillageId.Value) : null,
            m.TargetCoordinates
        )).ToList();

        return Result<IReadOnlyList<MovementDto>>.Success(dtos);
    }
}
