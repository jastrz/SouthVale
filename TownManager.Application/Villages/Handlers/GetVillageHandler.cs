using MediatR;
using TownManager.Application.Interfaces;
using TownManager.Application.Villages.Queries;

namespace TownManager.Application.Villages.Handlers;

public class GetVillageHandler(IVillageRepository repo) : IRequestHandler<GetVillageQuery, VillageDto>
{
    public async Task<VillageDto> Handle(GetVillageQuery q, CancellationToken ct)
    {
        var village = await repo.GetWithBuildingsAsync(q.VillageId, ct);
        
        var dto = new VillageDto(
            village.Id,
            village.Name,
            new ResourcesDto(village.Resources.Wood, village.Resources.Clay, village.Resources.Iron, village.Resources.Crop),
            new TroopsDto(village.Troops.Swordsmen, village.Troops.Archers),
            village.Buildings.Select(b => new BuildingDto(b.Id, b.Type.ToString(), b.Level)).ToList()
        );

        return dto;
    }
}