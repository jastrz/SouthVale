namespace TownManager.Application.Interfaces;

public interface IGameNotificationService
{
    Task VillageUpdatedAsync(string userId, Guid villageId, CancellationToken ct = default);
    Task VillagesChangedAsync(string userId, CancellationToken ct = default);
    Task MovementsChangedAsync(string userId, CancellationToken ct = default);
    Task ReportCreatedAsync(string userId, CancellationToken ct = default);
}
