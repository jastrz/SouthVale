using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Interfaces;

namespace TownManager.Application.Villages.Commands;

public class RenameVillageCommandHandler(IVillageRepository villageRepo)
    : IRequestHandler<RenameVillageCommand, Result>
{
    public async Task<Result> Handle(RenameVillageCommand request, CancellationToken ct)
    {
        var village = await villageRepo.GetWithActiveOrdersAsync(request.VillageId, ct);
        if (village is null)
            return Result.Failure(["Village not found."], statusCode: 404);

        village.Name = request.NewName;
        await villageRepo.SaveChangesAsync(ct);

        VillageActivity.Log?.Invoke(village.PlayerId.ToString(), request.NewName, "rename",
            new { OldName = village.Name });

        return Result.Success();
    }
}
