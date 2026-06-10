using Microsoft.EntityFrameworkCore;
using TownManager.Application.Interfaces;
using TownManager.Domain.Entities;

namespace TownManager.Infrastructure.Persistence.Repositories;

internal sealed class PlayerRepository(AppDbContext db) : IPlayerRepository
{
    public Task<Player?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Players.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<Player?> GetByUsernameAsync(string username, CancellationToken ct = default) =>
        db.Players.FirstOrDefaultAsync(p => p.Username == username, ct);

    public Task<bool> ExistsAsync(string username, CancellationToken ct = default) =>
        db.Players.AnyAsync(p => p.Username == username, ct);

    public void Add(Player player) => db.Players.Add(player);
}
