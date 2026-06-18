using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Interfaces;
using TownManager.Domain.Config;

namespace TownManager.Application.Villages.Queries;

public class GetCurrentUserVillagesQueryHandler(
    IPlayerRepository playerRepo,
    IVillageRepository villageRepo)
    : IRequestHandler<GetCurrentUserVillagesQuery, Result<IReadOnlyList<VillageDto>>>
{
    public async Task<Result<IReadOnlyList<VillageDto>>> Handle(
        GetCurrentUserVillagesQuery q, CancellationToken ct)
    {
        var player = await playerRepo.GetByUserIdAsync(q.UserId, ct);
        if (player is null)
            return Result<IReadOnlyList<VillageDto>>.Failure(["Player not found."]);

        var villages = await villageRepo.GetFullDetailsByPlayerAsync(player.Id, ct);

        var dtos = villages.Select(v =>
        {
            var effects = BuildingConfig.AggregateEffects(v.Buildings);
            var current = v.GetCurrentResources(effects);
            return new VillageDto(
                v.Id,
                v.Name,
                new ResourcesDto((int)current.Wood, (int)current.Clay, (int)current.Iron, (int)current.Crop),
                new TroopsDto(v.Troops.Swordsmen, v.Troops.Archers, v.Troops.Settlers),
                v.Buildings.Select(b => new BuildingDto(b.Id, b.Type.ToString(), b.Level)).ToList(),
                [],
                [],
                v.Coordinates
            );
        }).ToList();

        return Result<IReadOnlyList<VillageDto>>.Success(dtos);
    }
}
