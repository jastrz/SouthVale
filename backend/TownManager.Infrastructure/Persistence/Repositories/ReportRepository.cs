using Microsoft.EntityFrameworkCore;
using TownManager.Application.Interfaces;
using TownManager.Domain.Entities;

namespace TownManager.Infrastructure.Persistence.Repositories;

internal sealed class ReportRepository(AppDbContext db) : IReportRepository
{
    public async Task AddAsync(Report report, CancellationToken ct = default)
    {
        await db.Reports.AddAsync(report, ct);
    }

    public async Task<IReadOnlyList<Report>> GetByPlayerAsync(Guid playerId, int limit = 50, CancellationToken ct = default) =>
        await db.Reports
            .AsNoTracking()
            .Where(r => r.PlayerId == playerId)
            .OrderByDescending(r => r.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);

    public async Task<int> GetUnreadCountAsync(Guid playerId, CancellationToken ct = default) =>
        await db.Reports
            .AsNoTracking()
            .CountAsync(r => r.PlayerId == playerId && !r.IsRead, ct);

    public async Task MarkAsReadAsync(Guid reportId, Guid playerId, CancellationToken ct = default)
    {
        await db.Reports
            .Where(r => r.Id == reportId && r.PlayerId == playerId)
            .ExecuteUpdateAsync(u => u.SetProperty(r => r.IsRead, true), ct);
    }

    public async Task MarkAllAsReadAsync(Guid playerId, CancellationToken ct = default)
    {
        await db.Reports
            .Where(r => r.PlayerId == playerId && !r.IsRead)
            .ExecuteUpdateAsync(u => u.SetProperty(r => r.IsRead, true), ct);
    }
}
