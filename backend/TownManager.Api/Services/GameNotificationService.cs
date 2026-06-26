using Microsoft.AspNetCore.SignalR;
using TownManager.Api.Hubs;
using TownManager.Application.Interfaces;

namespace TownManager.Api.Services;

public class GameNotificationService(IHubContext<GameHub> hub) : IGameNotificationService
{
    public Task VillageUpdatedAsync(string userId, Guid villageId, CancellationToken ct = default) =>
        hub.Clients.User(userId).SendAsync("VillageUpdated", villageId, ct);

    public Task VillagesChangedAsync(string userId, CancellationToken ct = default) =>
        hub.Clients.User(userId).SendAsync("VillagesChanged", ct);

    public Task MovementsChangedAsync(string userId, CancellationToken ct = default) =>
        hub.Clients.User(userId).SendAsync("MovementsChanged", ct);

    public Task ReportCreatedAsync(string userId, CancellationToken ct = default) =>
        hub.Clients.User(userId).SendAsync("ReportCreated", ct);
}
