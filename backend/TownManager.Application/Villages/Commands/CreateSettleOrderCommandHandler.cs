using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Interfaces;
using TownManager.Domain.Config;
using TownManager.Domain.Entities;
using TownManager.Domain.Entities.Villages;
using TownManager.Domain.Enums;
using TownManager.Domain.Services;

namespace TownManager.Application.Villages.Commands;

public class CreateSettleOrderCommandHandler(
    IVillageRepository repo,
    IPlayerRepository playerRepo,
    IGameNotificationService notifications,
    IJobScheduler scheduler)
    : IRequestHandler<CreateSettleOrderCommand, Result>
{
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

        var speed = TroopsConfig.All[TroopType.Settler].Stats.Speed;
        var travelTime = TravelTimeCalculator.Calculate(village.Coordinates, request.Target, speed);

        var movement = TroopMovement.CreateSettle(
            settlersNeeded, request.Target, travelTime, DateTime.UtcNow);

        village.TroopMovements.Add(movement);

        await repo.SaveChangesAsync(ct);

        scheduler.ScheduleMovementResolution(movement.Id, travelTime);

        VillageActivity.Log?.Invoke(village.PlayerId.ToString(), village.Name, "settle",
            new { Target = request.Target });

        var userId = await playerRepo.GetUserIdByPlayerIdAsync(village.PlayerId, ct);
        if (userId is not null)
            await notifications.VillageUpdatedAsync(userId, village.Id, ct);

        return Result.Success();
    }
}
