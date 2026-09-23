using Microsoft.EntityFrameworkCore;
using TownManager.Application.Interfaces;
using TownManager.Domain.Entities;

namespace TownManager.Infrastructure.Persistence.Repositories;

internal sealed class WorldIterationRepository(AppDbContext db) : IWorldIterationRepository
{
    public Task<WorldIteration?> GetCurrentAsync(CancellationToken ct = default) =>
        db.WorldIterations.AsNoTracking().FirstOrDefaultAsync(i => i.EndedAt == null, ct);

    public async Task<IReadOnlyList<WorldIteration>> GetFinishedAsync(CancellationToken ct = default) =>
        await db.WorldIterations.AsNoTracking()
            .Where(i => i.EndedAt != null)
            .OrderBy(i => i.Number)
            .ToListAsync(ct);
}
