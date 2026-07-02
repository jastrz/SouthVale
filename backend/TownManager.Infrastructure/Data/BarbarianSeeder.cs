using TownManager.Domain.Config;
using TownManager.Domain.Entities;
using TownManager.Infrastructure.Persistence;

namespace TownManager.Infrastructure.Data;

public class BarbarianSeeder(AppDbContext db)
{
    public async Task SeedAsync()
    {
        if (!db.Players.Any(p => p.Id == BarbarianConfig.BarbarianPlayerId))
        {
            db.Players.Add(new Player
            {
                Id = BarbarianConfig.BarbarianPlayerId,
                Username = "Barbarian",
                UserId = null,
            });
            await db.SaveChangesAsync();
        }
    }
}
