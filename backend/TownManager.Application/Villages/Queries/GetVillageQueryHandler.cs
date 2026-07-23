using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Dtos;
using TownManager.Application.Interfaces;
using TownManager.Domain.Config;
using TownManager.Domain.Enums;

namespace TownManager.Application.Villages.Queries;

public class GetVillageQueryHandler(IVillageRepository repo)
    : IRequestHandler<GetVillageQuery, Result<VillageDto>>
{
    public async Task<Result<VillageDto>> Handle(GetVillageQuery q, CancellationToken ct)
    {
        var village = await repo.GetWithActiveOrdersAsync(q.VillageId, ct);

        if (village is null)
            return Result<VillageDto>.Failure(["Village not found."], statusCode: 404);

        var effects = BuildingConfig.AggregateEffects(village.Buildings);
        var current = village.GetCurrentResources(effects);

        return Result<VillageDto>.Success(new VillageDto(
            village.Id,
            village.Name,
            new ResourcesDto((int)current.Wood, (int)current.Clay, (int)current.Iron, (int)current.Beer),
            new TroopsDto(village.Troops.Get(TroopType.Swordsman), village.Troops.Get(TroopType.Archer), village.Troops.Get(TroopType.Settler), village.Troops.Get(TroopType.Dogs), village.Troops.Get(TroopType.Horsemen), village.Troops.Get(TroopType.LlamaRiders)),
            village.Buildings.Select(b => new BuildingDto(b.Id, b.Type.ToString(), b.Level)).ToList(),
            village.BuildOrders.Select(o => new BuildOrderDto(o.Id, o.BuildingType, o.TargetLevel, o.StartsAt, o.CompletesAt)).ToList(),
            village.TrainOrders.Select(o => new TrainOrderDto(o.Id, o.Type, o.Amount, o.Completed, o.StartsAt, o.CompletesAt)).ToList(),
            village.Coordinates
        ));
    }
}
