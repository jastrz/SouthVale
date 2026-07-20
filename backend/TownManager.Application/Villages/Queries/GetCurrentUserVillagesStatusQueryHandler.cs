using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Dtos;
using TownManager.Application.Interfaces;
using TownManager.Domain.Config;
using TownManager.Domain.Enums;

namespace TownManager.Application.Villages.Queries;

public class GetCurrentUserVillagesStatusQueryHandler(
    IPlayerRepository playerRepo,
    IVillageRepository villageRepo)
    : IRequestHandler<GetCurrentUserVillagesStatusQuery, Result<IReadOnlyList<VillageStatusDto>>>
{
    public async Task<Result<IReadOnlyList<VillageStatusDto>>> Handle(
        GetCurrentUserVillagesStatusQuery q, CancellationToken ct)
    {
        var player = await playerRepo.GetByUserIdAsync(q.UserId, ct);
        if (player is null)
            return Result<IReadOnlyList<VillageStatusDto>>.Failure(["Player not found."], statusCode: 404);

        var villages = await villageRepo.GetWithOrdersByPlayerAsync(player.Id, ct);

        var dtos = villages.Select(v =>
        {
            var effects = BuildingConfig.AggregateEffects(v.Buildings);
            var current = v.GetCurrentResources(effects);
            return new VillageStatusDto(
                v.Id,
                v.BuildOrders.Count,
                v.TrainOrders.Count,
                new ResourcesDto((int)current.Wood, (int)current.Clay, (int)current.Iron, (int)current.Beer),
                new TroopsDto(v.Troops.Get(TroopType.Swordsman), v.Troops.Get(TroopType.Archer), v.Troops.Get(TroopType.Settler), v.Troops.Get(TroopType.Dogs), v.Troops.Get(TroopType.Horsemen), v.Troops.Get(TroopType.LlamaRiders))
            );
        }).ToList();

        return Result<IReadOnlyList<VillageStatusDto>>.Success(dtos);
    }
}
