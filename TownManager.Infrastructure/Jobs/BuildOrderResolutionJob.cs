using Hangfire;
using TownManager.Application.Interfaces;

namespace TownManager.Infrastructure.Jobs;

public class BuildOrderResolutionJob(IVillageRepository repo, IUnitOfWork uow)
{
    [AutomaticRetry(Attempts = 3)]
    [DisableConcurrentExecution(60)]
    public async Task ResolveAsync(Guid orderId, CancellationToken ct)
    {
        var village = await repo.GetWithBuildingsAndOrdersAsync(orderId, ct);
        if (village is null) return;

        var order = village.BuildOrders.FirstOrDefault(o => o.Id == orderId);
        if (order is null) return;

        var building = village.Buildings.First(b => b.Type == order.BuildingType);
        building.Level = order.TargetLevel;

        village.BuildOrders.Remove(order);

        await uow.SaveChangesAsync(ct);
    }
}