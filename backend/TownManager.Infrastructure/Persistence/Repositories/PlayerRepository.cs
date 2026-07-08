using Microsoft.EntityFrameworkCore;
using TownManager.Application.Interfaces;
using TownManager.Domain.Config;
using TownManager.Domain.Entities;

namespace TownManager.Infrastructure.Persistence.Repositories;

internal sealed class PlayerRepository(AppDbContext db) : IPlayerRepository
{
    public Task<Player?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Players.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<Player?> GetByUsernameAsync(string username, CancellationToken ct = default) =>
        db.Players.FirstOrDefaultAsync(p => p.Username == username, ct);

    public Task<Player?> GetByUserIdAsync(string userId, CancellationToken ct = default) =>
        db.Players.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == userId, ct);

    public Task<bool> ExistsAsync(string username, CancellationToken ct = default) =>
        db.Players.AnyAsync(p => p.Username == username, ct);

    public Task<string?> GetUserIdByPlayerIdAsync(Guid playerId, CancellationToken ct = default) =>
        db.Players.Where(p => p.Id == playerId).Select(p => p.UserId).FirstOrDefaultAsync(ct);

    public Task<string?> GetUserIdByVillageIdAsync(Guid villageId, CancellationToken ct = default) =>
        db.Players.Where(p => p.Villages.Any(v => v.Id == villageId)).Select(p => p.UserId).FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<Player>> GetBotPlayersAsync(CancellationToken ct = default) =>
        await db.Players
            .AsNoTracking()
            .Include(p => p.Villages)
            .Where(p => p.IsBot)
            .ToListAsync(ct);

    public void Add(Player player) => db.Players.Add(player);

    public async Task<IReadOnlyList<Player>> GetAllPlayersWithTroopDataAsync(CancellationToken ct = default) =>
        await db.Players
            .AsNoTracking()
            .Include(p => p.Villages)
                .ThenInclude(v => v.TroopMovements)
            .Where(p => p.Id != BarbarianConfig.BarbarianPlayerId)
            .ToListAsync(ct);
}
