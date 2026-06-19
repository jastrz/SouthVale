using Microsoft.EntityFrameworkCore;
using TownManager.Application.Interfaces;
using TownManager.Domain.Entities;
using TownManager.Domain.Entities.Villages;
using TownManager.Domain.Enums;

namespace TownManager.Infrastructure.Persistence.Repositories;

internal sealed class VillageRepository(AppDbContext db) : IVillageRepository
{
    // By villageId
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
            .Include(v => v.Buildings)
            .FirstOrDefaultAsync(v => v.Id == id, ct);

    public Task<Village?> GetForCombatAsync(Guid id, CancellationToken ct = default) =>
        db.Villages
            .Include(v => v.Troops)
            .Include(v => v.Resources)
            .FirstOrDefaultAsync(v => v.Id == id, ct);

    public Task<Village?> GetWithMovementOrdersAsync(Guid id, CancellationToken ct = default) =>
        db.Villages
            .Include(v => v.Troops)
            .Include(v => v.TroopMovements)
            .FirstOrDefaultAsync(v => v.Id == id, ct);

    // By Hangire orderId
    public Task<Village?> GetWithBuildingsAndOrdersAsync(Guid orderId, CancellationToken ct = default) =>
        db.Villages
            .Include(v => v.Buildings)
            .Include(v => v.BuildOrders)
            .AsSplitQuery()
            .FirstOrDefaultAsync(v => v.BuildOrders.Any(o => o.Id == orderId), ct);

    public Task<Village?> GetWithTrainOrdersAsync(Guid orderId, CancellationToken ct) =>
        db.Villages
            .Include(v => v.TrainOrders)
            .FirstOrDefaultAsync(v => v.TrainOrders.Any(o => o.Id == orderId), ct);

    public Task<Village?> GetWithAttackOrdersAsyncByOrder(Guid orderId, CancellationToken ct = default) =>
        db.Villages
            .Include(v => v.Troops)
            .Include(v => v.TroopMovements)
            .FirstOrDefaultAsync(v => v.TroopMovements.Any(o => o.Id == orderId), ct);

    // By playerId

    public async Task<IReadOnlyList<Village>> GetSummariesByPlayerAsync(Guid playerId, CancellationToken ct = default) =>
        await db.Villages
            .AsNoTracking()
            .Include(v => v.Troops)
            .Where(v => v.PlayerId == playerId)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Village>> GetFullDetailsByPlayerAsync(Guid playerId, CancellationToken ct = default) =>
        await db.Villages
            .AsNoTracking()
            .Include(v => v.Buildings)
            .Include(v => v.Troops)
            .Include(v => v.Resources)
            .Where(v => v.PlayerId == playerId)
            .ToListAsync(ct);

    // By map coordinates
    public Task<Village?> GetByCoordsAsync(Coordinates coordinates, CancellationToken ct = default) =>
        db.Villages
            .AsNoTracking()
            .FirstOrDefaultAsync(v =>
                v.Coordinates.Equals(coordinates),
                ct);

    // Lookups
    public async Task<IReadOnlyDictionary<Guid, string>> GetNamesByIdsAsync(
        IReadOnlyCollection<Guid> ids, CancellationToken ct = default) =>
        await db.Villages
            .AsNoTracking()
            .Where(v => ids.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id, v => v.Name, ct);
    
    public async Task<IReadOnlyList<Village>> GetForMapWithinRadius(
        Coordinates? center = null,
        int? radius = null,
        CancellationToken ct = default)
    {
        var query = db.Villages.AsNoTracking();

        if (center is not null && radius is not null)
        {
            var r = radius.Value;
            var r2 = r * r;
            query = query.Where(v =>
                v.Coordinates.X >= center.X - r && v.Coordinates.X <= center.X + r &&
                v.Coordinates.Y >= center.Y - r && v.Coordinates.Y <= center.Y + r &&
                (v.Coordinates.X - center.X) * (v.Coordinates.X - center.X) +
                (v.Coordinates.Y - center.Y) * (v.Coordinates.Y - center.Y) <= r2);
        }

        return await query.ToListAsync(ct);
    }
    
    public async Task<int?> GetMaxBuildOrderTargetAsync(Guid villageId, BuildingType type, CancellationToken ct = default) =>
        await db.BuildOrders
            .Where(o => o.VillageId == villageId && o.BuildingType == type)
            .MaxAsync(o => (int?)o.TargetLevel, ct);

    public void Add(Village village) => db.Villages.Add(village);
    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}