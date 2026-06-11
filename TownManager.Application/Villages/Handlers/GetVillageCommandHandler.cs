using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Interfaces;
using TownManager.Application.Villages.Queries;
using TownManager.Domain.Config;

namespace TownManager.Application.Villages.Handlers;

public class GetVillageCommandHandler(IVillageRepository repo, IUnitOfWork uow) 
    : IRequestHandler<GetVillageCommand, Result<VillageDto>>
{
    public async Task<Result<VillageDto>> Handle(GetVillageCommand q, CancellationToken ct)
    {
        var village = await repo.GetWithBuildingsAsync(q.VillageId, ct);

        if (village is null)
            return Result<VillageDto>.Failure(["Village not found"]);

        var effects = BuildingConfig.AggregateEffects(village.Buildings);
        village.ApplyProduction(effects);
        
        try
        {
            await uow.SaveChangesAsync(ct);
        }
        catch (Exception e)
        {
            return Result<VillageDto>.Failure([$"Failed to save village: {e.Message}"]);
        }

        return Result<VillageDto>.Success(new VillageDto(
            village.Id,
            village.Name,
            new ResourcesDto((int)village.Resources.Wood, (int)village.Resources.Clay, (int)village.Resources.Iron, (int)village.Resources.Crop),
            new TroopsDto(village.Troops.Swordsmen, village.Troops.Archers),
            village.Buildings.Select(b => new BuildingDto(b.Id, b.Type.ToString(), b.Level)).ToList()
        ));
    }
}