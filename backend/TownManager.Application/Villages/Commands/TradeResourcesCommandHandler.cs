using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Interfaces;
using TownManager.Domain.Config;
using TownManager.Domain.Entities;
using TownManager.Domain.Enums;

namespace TownManager.Application.Villages.Commands;

public record TradeResourcesCommand(
    Guid VillageId,
    ResourceType GiveType,
    int GiveAmount,
    ResourceType ReceiveType
) : IRequest<Result>;

public class TradeResourcesCommandHandler(
    IVillageRepository repo,
    IGameNotificationService notifications,
    IPlayerRepository playerRepo
) : IRequestHandler<TradeResourcesCommand, Result>
{
    public async Task<Result> Handle(TradeResourcesCommand request, CancellationToken ct)
    {
        var village = await repo.GetForCombatAsync(request.VillageId, ct);
        if (village is null)
            return Result.Failure(["Village not found."], statusCode: 404);

        if (request.GiveType == request.ReceiveType)
            return Result.Failure(["Cannot trade a resource for itself."]);

        if (request.GiveAmount <= 0)
            return Result.Failure(["Amount must be positive."]);

        var effects = BuildingConfig.AggregateEffects(village.Buildings);
        village.ApplyProduction(effects);

        var hasTradePost = village.Buildings.Any(b => b.Type == BuildingType.TradePost && b.Level >= 1);
        if (!hasTradePost)
            return Result.Failure(["Trading post required. Build one first."]);

        var receiveAmount = (int)Math.Floor(request.GiveAmount * effects.TradeRate);
        if (receiveAmount < 1)
            return Result.Failure(["Trade rate too low. Upgrade trading post."]);

        var give = ResourceByType(request.GiveType, request.GiveAmount);
        var get = ResourceByType(request.ReceiveType, receiveAmount);

        if (!village.Resources.CanAfford(give))
            return Result.Failure(["Not enough resources."]);

        var currentGetAmount = request.ReceiveType switch
        {
            ResourceType.Wood => village.Resources.Wood,
            ResourceType.Clay => village.Resources.Clay,
            ResourceType.Iron => village.Resources.Iron,
            ResourceType.Beer => village.Resources.Beer,
            _ => 0
        };

        if (currentGetAmount + receiveAmount > effects.WarehouseCapacity)
            return Result.Failure(["Warehouse cannot hold that much of the received resource."]);

        village.Resources = village.Resources.Subtract(give).Add(get);
        await repo.SaveChangesAsync(ct);

        VillageActivity.Log?.Invoke(village.PlayerId.ToString(), village.Name, "trade",
            new { GiveType = request.GiveType.ToString(), request.GiveAmount, ReceiveType = request.ReceiveType.ToString(), receiveAmount, TradeRate = effects.TradeRate });

        var userId = await playerRepo.GetUserIdByPlayerIdAsync(village.PlayerId, ct);
        if (userId is not null)
            await notifications.VillageUpdatedAsync(userId, village.Id, ct);

        return Result.Success();
    }

    private static Resources ResourceByType(ResourceType type, int amount) => type switch
    {
        ResourceType.Wood => new Resources(amount, 0, 0, 0),
        ResourceType.Clay => new Resources(0, amount, 0, 0),
        ResourceType.Iron => new Resources(0, 0, amount, 0),
        ResourceType.Beer => new Resources(0, 0, 0, amount),
        _ => Resources.Zero
    };
}
