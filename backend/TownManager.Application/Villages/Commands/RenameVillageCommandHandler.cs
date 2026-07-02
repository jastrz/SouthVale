using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Interfaces;

namespace TownManager.Application.Villages.Commands;

public class RenameVillageCommandHandler(IPlayerRepository playerRepo, IVillageRepository villageRepo)
    : IRequestHandler<RenameVillageCommand, Result>
{
    public async Task<Result> Handle(RenameVillageCommand request, CancellationToken ct)
    {
        var player = await playerRepo.GetByUserIdAsync(request.UserId, ct);
        if (player is null)
            return Result.Failure(["Player not found."], statusCode: 404);

        var village = await villageRepo.GetWithActiveOrdersAsync(request.VillageId, ct);
        if (village is null)
            return Result.Failure(["Village not found."], statusCode: 404);

        if (village.PlayerId != player.Id)
            return Result.Failure(["You do not own this village."], statusCode: 403);

        village.Name = request.NewName;
        await villageRepo.SaveChangesAsync(ct);

        VillageActivity.Log?.Invoke(player.Id.ToString(), request.NewName, "rename",
            new { OldName = village.Name });

        return Result.Success();
    }
}
