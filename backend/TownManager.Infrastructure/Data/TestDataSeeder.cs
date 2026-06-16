using Microsoft.AspNetCore.Identity;
using TownManager.Domain.Entities;
using TownManager.Domain.Entities.Villages;
using TownManager.Infrastructure.Identity;
using TownManager.Infrastructure.Persistence;

namespace TownManager.Infrastructure.Data;

public class TestDataSeeder(UserManager<ApplicationUser> userManager, AppDbContext db)
{
    public async Task SeedAsync()
    {
        await SeedTestUserAsync("test1@test.com", "Test123!", "TestVillage1", (0, 0));
        await SeedTestUserAsync("test2@test.com", "Test123!", "TestVillage2", (0, 0));
    }

    private async Task SeedTestUserAsync(string email, string password, string villageName, (int X, int Y) coordinates)
    {
        // 1. Create ApplicationUser
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email
        };

        var result = await userManager.CreateAsync(user, password);
        
        if (!result.Succeeded)
            throw new Exception($"Failed to create user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");

        // 2. Create Player + Village
        await using var transaction = await db.Database.BeginTransactionAsync();

        try
        {
            var village = Village.CreateStarter(villageName, coordinates);
            var player = Player.Create(
                username: email.Split('@')[0],
                applicationUserId: user.Id,
                starterVillage: village
            );

            db.Villages.Add(village);
            db.Players.Add(player);

            await db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            // Clean up the user if game data fails
            await userManager.DeleteAsync(user);
            throw;
        }
    }
}
