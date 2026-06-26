using TownManager.Domain.Entities;

namespace TownManager.Application.Interfaces;

public interface IReportRepository
{
    Task AddAsync(Report report, CancellationToken ct = default);
    Task<IReadOnlyList<Report>> GetByPlayerAsync(Guid playerId, int skip = 0, int limit = 50, CancellationToken ct = default);
    Task<int> GetTotalCountAsync(Guid playerId, CancellationToken ct = default);
    Task<int> GetUnreadCountAsync(Guid playerId, CancellationToken ct = default);
    Task MarkAsReadAsync(Guid reportId, Guid playerId, CancellationToken ct = default);
    Task MarkAllAsReadAsync(Guid playerId, CancellationToken ct = default);
}
