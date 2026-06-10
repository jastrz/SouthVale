using Microsoft.EntityFrameworkCore;
using TownManager.Application.Interfaces;
using TownManager.Domain.Entities.Villages;

namespace TownManager.Infrastructure.Persistence.Repositories;

internal sealed class VillageRepository(AppDbContext db) : IVillageRepository
{
    public Task<Village?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Villages
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id, ct);

    public Task<Village?> GetWithBuildingsAsync(Guid id, CancellationToken ct = default) =>
        db.Villages
            .Include(v => v.Buildings)
            .FirstOrDefaultAsync(v => v.Id == id, ct);

    public Task<Village?> GetWithActiveOrdersAsync(Guid id, CancellationToken ct = default) =>
        db.Villages
            .Include(v => v.BuildOrders)
            .Include(v => v.TrainOrders)
            .AsSplitQuery()
            .FirstOrDefaultAsync(v => v.Id == id, ct);

    public Task<Village?> GetForCombatAsync(Guid id, CancellationToken ct = default) =>
        db.Villages
            .Include(v => v.Troops)
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id, ct);

    public async Task<IReadOnlyList<Village>> GetByPlayerAsync(Guid playerId, CancellationToken ct = default) =>
        await db.Villages
            .AsNoTracking()
            .Where(v => v.PlayerId == playerId)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Village>> GetVillagesCompletingBeforeAsync(DateTime cutoff, CancellationToken ct = default) =>
        await db.Villages
            .Include(v => v.BuildOrders.Where(o => o.CompletesAt <= cutoff))
            .Include(v => v.TrainOrders.Where(o => o.CompletesAt <= cutoff))
            .AsSplitQuery()
            .AsNoTracking()
            .Where(v => v.BuildOrders.Any(o => o.CompletesAt <= cutoff)
                     || v.TrainOrders.Any(o => o.CompletesAt <= cutoff))
            .ToListAsync(ct);

    public void Add(Village village) => db.Villages.Add(village);
}