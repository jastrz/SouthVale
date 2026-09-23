using Microsoft.EntityFrameworkCore;
using TownManager.Domain.Entities;
using TownManager.Infrastructure.Persistence;

namespace TownManager.Infrastructure.Data;

public class WorldIterationSeeder(AppDbContext db)
{
    public async Task SeedAsync()
    {
        if (await db.WorldIterations.AnyAsync(i => i.EndedAt == null)) return;

        var lastNumber = await db.WorldIterations.MaxAsync(i => (int?)i.Number) ?? 0;
        db.WorldIterations.Add(new WorldIteration
        {
            Number = lastNumber + 1,
            StartedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();
    }
}
