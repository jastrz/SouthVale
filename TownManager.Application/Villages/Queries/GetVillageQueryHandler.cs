using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Interfaces;

namespace TownManager.Application.Villages.Queries;

public class GetVillageQueryHandler(IVillageRepository repo)
    : IRequestHandler<GetVillageQuery, Result<VillageDto>>
{
    public async Task<Result<VillageDto>> Handle(GetVillageQuery q, CancellationToken ct)
    {
        var village = await repo.GetWithBuildingsAsync(q.VillageId, ct);

        if (village is null)
            return Result<VillageDto>.Failure(["Village not found."]);

        return Result<VillageDto>.Success(new VillageDto(
            village.Id,
            village.Name,
            new ResourcesDto((int)village.Resources.Wood, (int)village.Resources.Clay, (int)village.Resources.Iron, (int)village.Resources.Crop),
            new TroopsDto(village.Troops.Swordsmen, village.Troops.Archers),
            village.Buildings.Select(b => new BuildingDto(b.Id, b.Type.ToString(), b.Level)).ToList()
        ));
    }
}