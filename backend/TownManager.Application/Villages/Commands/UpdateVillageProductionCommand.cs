using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Interfaces;
using TownManager.Domain.Config;

namespace TownManager.Application.Villages.Commands;

public record UpdateVillageProductionCommand(Guid VillageId, Guid UserId) : IRequest<Result>;

public class UpdateVillageProductionCommandHandler(IVillageRepository repo, IUnitOfWork uow)
    : IRequestHandler<UpdateVillageProductionCommand, Result>
{
    public async Task<Result> Handle(UpdateVillageProductionCommand cmd, CancellationToken ct)
    {
        var village = await repo.GetWithBuildingsAsync(cmd.VillageId, ct);

        if (village is null)
            return Result.Failure(["Village not found."]);

        // if (village.PlayerId != cmd.UserId)
        //     return Result.Failure(ErrorType.Forbidden, ["Not your village."]);

        var effects = BuildingConfig.AggregateEffects(village.Buildings);
        village.ApplyProduction(effects);

        await uow.SaveChangesAsync(ct);
        
        return Result.Success();
    }
}