using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Interfaces;

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

        var dtos = villages.Select(v => new VillageDto(
            v.Id,
            v.Name,
            new ResourcesDto((int)v.Resources.Wood, (int)v.Resources.Clay, (int)v.Resources.Iron, (int)v.Resources.Crop),
            new TroopsDto(v.Troops.Swordsmen, v.Troops.Archers, v.Troops.Settlers),
            v.Buildings.Select(b => new BuildingDto(b.Id, b.Type.ToString(), b.Level)).ToList()
        )).ToList();

        return Result<IReadOnlyList<VillageDto>>.Success(dtos);
    }
}
