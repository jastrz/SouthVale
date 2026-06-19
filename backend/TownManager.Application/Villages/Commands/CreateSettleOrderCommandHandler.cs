using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Interfaces;
using TownManager.Domain.Entities;
using TownManager.Domain.Entities.Villages;

namespace TownManager.Application.Villages.Commands;

public class CreateSettleOrderCommandHandler(
    IVillageRepository repo,
    IJobScheduler scheduler)
    : IRequestHandler<CreateSettleOrderCommand, Result>
{
    // TODO: move to game config later
    private const int SecondsPerField = 2;

    public async Task<Result> Handle(CreateSettleOrderCommand request, CancellationToken ct)
    {
        var village = await repo.GetWithMovementOrdersAsync(request.VillageId, ct);
        if (village is null)
            return Result.Failure(["Village not found."], statusCode: 404);

        var settlersNeeded = new Troops(0, 0, 1);
        if (!village.Troops.HasEnough(settlersNeeded))
            return Result.Failure(["Not enough settlers in garrison."]);

        if (await repo.GetByCoordsAsync(request.Target, ct) is not null)
            return Result.Failure(["Target tile is already occupied."], statusCode: 409);

        village.Troops = village.Troops.Subtract(settlersNeeded);

        var distance = Math.Abs(village.Coordinates.X - request.Target.X) + Math.Abs(village.Coordinates.Y - request.Target.Y);
        var travelTime = TimeSpan.FromSeconds(distance * SecondsPerField);

        var movement = TroopMovement.CreateSettle(
            settlersNeeded, request.Target, travelTime, DateTime.UtcNow);

        village.TroopMovements.Add(movement);

        await repo.SaveChangesAsync(ct);

        scheduler.ScheduleMovementResolution(movement.Id, travelTime);

        return Result.Success();
    }
}
