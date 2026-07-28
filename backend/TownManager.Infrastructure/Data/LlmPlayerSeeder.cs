using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TownManager.Application.Map.Services;
using TownManager.Domain.Entities;
using TownManager.Domain.Entities.Villages;
using TownManager.Domain.Enums;
using TownManager.Infrastructure.Identity;
using TownManager.Infrastructure.Persistence;

namespace TownManager.Infrastructure.Data;

public class LlmPlayerSeeder(UserManager<ApplicationUser> userManager, AppDbContext db,
    IMapService mapService, ILogger<LlmPlayerSeeder> logger)
{
    private static readonly List<(string Username, string Email, BotPersonality Personality)> Seeds =
    [
        ("wściekły_bobas",  "wsciekly_bobas@somemail.com",  BotPersonality.Aggressive),
        ("Krowa500",  "krowa500@somemail.com",  BotPersonality.Defensive),
        ("kasztelan",   "kasztelan@somemail.com",   BotPersonality.Economic),
        ("kargul666",  "kargul@somemail.com",  BotPersonality.Aggressive),
        ("siórmistrz",  "siormistrz@somemail.com",  BotPersonality.Defensive),
        ("krasnal",   "krasnal@somemail.com",   BotPersonality.Economic),
    ];

    public async Task SeedAsync()
    {
        var existing = await db.Players.Where(p => p.IsBot).Select(p => p.Username).ToListAsync();
        var newSeeds = Seeds.Where(s => !existing.Contains(s.Username)).ToList();
        if (newSeeds.Count == 0) return;

        var free = await mapService.GetFreeTilesAsync(newSeeds.Count, CancellationToken.None);

        for (var i = 0; i < newSeeds.Count; i++)
        {
            var (username, email, personality) = newSeeds[i];
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = email,
                Email = email,
            };

            var result = await userManager.CreateAsync(user, "BotPassword123!");
            if (!result.Succeeded)
            {
                logger.LogWarning("Failed to create LLM user {Email}: {Errors}", email,
                    string.Join(", ", result.Errors.Select(e => e.Description)));
                continue;
            }

            var coords = i < free.Count ? free[i] : new Coordinates(0, 0);
            var village = Village.CreateStarter($"{username}'s camp", coords);
            var player = Player.CreateBot(username, user.Id, village, personality);

            db.Players.Add(player);

            logger.LogInformation("LLM player seeded: {Username} ({Personality}) at ({X},{Y})",
                username, personality, coords.X, coords.Y);
        }

        await db.SaveChangesAsync();
    }
}
