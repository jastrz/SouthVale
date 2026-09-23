using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TownManager.Application.Leaderboard.Queries;
using TownManager.Application.Map.Services;
using TownManager.Application.World;
using TownManager.Domain.Entities;
using TownManager.Domain.Entities.Villages;
using TownManager.Infrastructure.Identity;

namespace TownManager.Infrastructure.Persistence;

public class WorldResetService(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    IMapService mapService,
    IMediator mediator,
    ILogger<WorldResetService> logger) : IWorldResetService
{
    private const int WinnerCount = 3;

    public Task<WorldIteration?> GetCurrentIterationAsync(CancellationToken ct = default) =>
        db.WorldIterations.FirstOrDefaultAsync(i => i.EndedAt == null, ct);

    public async Task<WorldResetResult> ResetAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        // Snapshot before the transaction: the query goes through TransactionBehavior,
        // which cannot nest inside the reset transaction below.
        var leaderboard = await mediator.Send(new GetLeaderboardQuery(1, WinnerCount), ct);
        var winners = leaderboard.Value?.Items
            .Select(e => new WorldIterationWinner { Username = e.Username, Score = e.Score })
            .ToList() ?? [];

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var current = await GetCurrentIterationAsync(ct);
        if (current is not null)
        {
            current.EndedAt = now;
            current.Winners = winners;
            await db.SaveChangesAsync(ct);
        }

        var keptUsers = await userManager.Users
            .Where(u => u.Email != null)
            .Select(u => new { u.Id, u.UserName, u.Email })
            .ToListAsync(ct);

        var keptIds = keptUsers.Select(u => u.Id).ToHashSet();
        var keptPlayers = await db.Players
            .Where(p => keptIds.Contains(p.UserId!))
            .Select(p => new { p.UserId, p.Username, p.IsBot })
            .ToListAsync(ct);

        var keptUserNames = keptPlayers.Where(p => !p.IsBot).ToDictionary(p => p.UserId!, p => p.Username);
        var botUserIds = keptPlayers.Where(p => p.IsBot).Select(p => p.UserId!).ToHashSet();

        await db.Database.ExecuteSqlRawAsync("""
            DELETE FROM "AspNetUserTokens" WHERE "UserId" IN (SELECT "Id" FROM "AspNetUsers" WHERE "Email" IS NULL);
            DELETE FROM "AspNetUserLogins" WHERE "UserId" IN (SELECT "Id" FROM "AspNetUsers" WHERE "Email" IS NULL);
            DELETE FROM "AspNetUserClaims" WHERE "UserId" IN (SELECT "Id" FROM "AspNetUsers" WHERE "Email" IS NULL);
            DELETE FROM "AspNetUserRoles" WHERE "UserId" IN (SELECT "Id" FROM "AspNetUsers" WHERE "Email" IS NULL);
            DELETE FROM "AspNetUsers" WHERE "Email" IS NULL;
            DELETE FROM "Reports";
            DELETE FROM "Villages";
            DELETE FROM "Players";
            DROP SCHEMA IF EXISTS hangfire CASCADE;
            """, ct);

        db.ChangeTracker.Clear();

        foreach (var botId in botUserIds)
        {
            var bot = await userManager.FindByIdAsync(botId);
            if (bot is not null) await userManager.DeleteAsync(bot);
        }

        var adminIds = (await userManager.GetUsersInRoleAsync("Admin"))
            .Select(u => u.Id)
            .ToHashSet();

        var nonAdminKept = keptUsers.Where(u => !adminIds.Contains(u.Id) && !botUserIds.Contains(u.Id)).ToList();

        if (nonAdminKept.Count > 0)
        {
            var freeTiles = await mapService.GetFreeTilesAsync(nonAdminKept.Count, ct);
            for (var i = 0; i < nonAdminKept.Count; i++)
            {
                var kept = nonAdminKept[i];
                var username = keptUserNames.GetValueOrDefault(kept.Id) ?? kept.UserName ?? kept.Email ?? $"Player_{i}";
                var coords = i < freeTiles.Count ? freeTiles[i] : new Coordinates(0, 0);
                var village = Village.CreateStarter($"{username}'s village", coords);
                var player = Player.Create(username, kept.Id, village);
                db.Players.Add(player);
                db.Villages.Add(village);
            }
        }

        var next = new WorldIteration
        {
            Number = (current?.Number ?? 0) + 1,
            StartedAt = now,
        };
        db.WorldIterations.Add(next);

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        logger.LogWarning(
            "Soft world reset: iteration {Previous} ended, iteration {Next} started — {Users} users kept, {Players} players recreated",
            current?.Number, next.Number, keptUsers.Count, nonAdminKept.Count);

        return new WorldResetResult(next.Number, keptUsers.Count, nonAdminKept.Count);
    }
}
