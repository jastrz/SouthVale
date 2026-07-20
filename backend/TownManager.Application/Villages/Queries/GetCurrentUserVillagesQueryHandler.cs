using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Dtos;
using TownManager.Application.Interfaces;
using TownManager.Domain.Config;

namespace TownManager.Application.Villages.Queries;

public class GetCurrentUserVillagesQueryHandler(
    IPlayerRepository playerRepo,
    IVillageRepository villageRepo)
    : IRequestHandler<GetCurrentUserVillagesQuery, Result<IReadOnlyList<VillageListItemDto>>>
{
    public async Task<Result<IReadOnlyList<VillageListItemDto>>> Handle(
        GetCurrentUserVillagesQuery q, CancellationToken ct)
    {
        var player = await playerRepo.GetByUserIdAsync(q.UserId, ct);
        if (player is null)
            return Result<IReadOnlyList<VillageListItemDto>>.Failure(["Player not found."], statusCode: 404);

        var villages = await villageRepo.GetFullDetailsByPlayerAsync(player.Id, ct);

        var dtos = villages.Select(v =>
        {
            var effects = BuildingConfig.AggregateEffects(v.Buildings);
            var current = v.GetCurrentResources(effects);
            return new VillageListItemDto(
                v.Id,
                v.Name,
                new ResourcesDto((int)current.Wood, (int)current.Clay, (int)current.Iron, (int)current.Beer),
                new TroopsDto(v.Troops.Swordsmen, v.Troops.Archers, v.Troops.Settlers),
                v.Coordinates
            );
        }).ToList();

        return Result<IReadOnlyList<VillageListItemDto>>.Success(dtos);
    }
}
