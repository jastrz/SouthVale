using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using NCrontab;
using TownManager.Application.Barbarians;
using TownManager.Application.Llm;
using TownManager.Application.Players.Services;
using TownManager.Application.Map.Services;
using TownManager.Application.Villages.Services;
using TownManager.Domain.Entities.Villages;
using TownManager.Domain.Config;
using TownManager.Domain.Entities;
using TownManager.Infrastructure.Identity;
using TownManager.Infrastructure.Jobs;
using TownManager.Infrastructure.Persistence;

namespace TownManager.Api.Endpoints.Admin;

public class AdminEndpoints : IEndpoint
{
    private static async Task<IResult?> AdminGuard(HttpContext httpContext, UserManager<ApplicationUser> userManager)
    {
        var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Results.Unauthorized();
        var user = await userManager.FindByIdAsync(userId);
        if (user is null || !await userManager.IsInRoleAsync(user, "Admin"))
            return Results.Forbid();
        return null;
    }

    public record ResetRequest(string Password);

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/villages", async (
            HttpContext httpContext,
            UserManager<ApplicationUser> userManager,
            AppDbContext db,
            CancellationToken ct
        ) =>
        {
            var guard = await AdminGuard(httpContext, userManager);
            if (guard is not null) return guard;

            var villages = await db.Villages
                .Select(v => new { v.Id, v.Name, v.Coordinates })
                .ToListAsync(ct);

            return Results.Ok(villages);
        })
        .WithName("AdminListVillages")
        .WithTags("Admin")
        .WithSummary("List all villages")
        .WithDescription("Returns id, name, and coordinates for every village in the database.")
        .RequireAuthorization();

        app.MapPost("/admin/village/{id:guid}/resources", async (
            Guid id,
            AddResourcesRequest request,
            HttpContext httpContext,
            UserManager<ApplicationUser> userManager,
            AppDbContext db,
            CancellationToken ct
        ) =>
        {
            var guard = await AdminGuard(httpContext, userManager);
            if (guard is not null) return guard;

            var village = await db.Villages
                .Include(v => v.Buildings)
                .FirstOrDefaultAsync(v => v.Id == id, ct);
            if (village is null) return Results.NotFound();

            var added = new Resources(request.Wood, request.Clay, request.Iron, request.Beer);
            var effects = BuildingConfig.AggregateEffects(village.Buildings);
            village.Resources = village.GetCurrentResources(effects).Add(added);
            village.LastTickAt = DateTime.UtcNow;

            await db.SaveChangesAsync(ct);
            return Results.Ok();
        })
        .WithName("AdminAddResources")
        .WithTags("Admin")
        .WithSummary("Add resources to a village")
        .WithDescription("Adds the specified amounts of wood, clay, iron, and crop to a village.")
        .RequireAuthorization();

        app.MapPost("/admin/villages/resources/full", async (
            HttpContext httpContext,
            UserManager<ApplicationUser> userManager,
            AppDbContext db,
            CancellationToken ct
        ) =>
        {
            var guard = await AdminGuard(httpContext, userManager);
            if (guard is not null) return guard;

            var villages = await db.Villages
                .Include(v => v.Buildings)
                .ToListAsync(ct);

            foreach (var village in villages)
            {
                var effects = BuildingConfig.AggregateEffects(village.Buildings);
                village.Resources = new Resources(
                    effects.WarehouseCapacity,
                    effects.WarehouseCapacity,
                    effects.WarehouseCapacity,
                    effects.WarehouseCapacity
                );
                village.LastTickAt = DateTime.UtcNow;
            }

            await db.SaveChangesAsync(ct);
            return Results.Ok(new { filled = villages.Count });
        })
        .WithName("AdminFullResources")
        .WithTags("Admin")
        .WithSummary("Set all villages to max resources")
        .WithDescription("Fills every village's warehouse and granary to full capacity. Returns count of villages filled.")
        .RequireAuthorization();

        app.MapPost("/admin/tick/barbarian", async (
            HttpContext httpContext,
            UserManager<ApplicationUser> userManager,
            IBarbarianTickService barbarianTick,
            FeatureFlags features,
            CancellationToken ct
        ) =>
        {
            var guard = await AdminGuard(httpContext, userManager);
            if (guard is not null) return guard;

            if (!features.UseBarbarians)
                return Results.BadRequest(new { error = "Barbarians feature is disabled" });

            await barbarianTick.ExecuteAsync(ct);
            return Results.Ok(new { ticked = "barbarian" });
        })
        .WithName("AdminTickBarbarian")
        .WithTags("Admin")
        .WithSummary("Run barbarian tick immediately")
        .WithDescription("Triggers the barbarian AI tick immediately instead of waiting for the cron schedule. Requires UseBarbarians feature flag.")
        .RequireAuthorization();

        app.MapPost("/admin/tick/llm", async (
            HttpContext httpContext,
            UserManager<ApplicationUser> userManager,
            IServiceScopeFactory scopeFactory,
            FeatureFlags features
        ) =>
        {
            var guard = await AdminGuard(httpContext, userManager);
            if (guard is not null) return guard;

            if (!features.UseLlmPlayers)
                return Results.BadRequest(new { error = "LLM players feature is disabled" });

            _ = Task.Run(async () =>
            {
                using var scope = scopeFactory.CreateScope();
                var tick = scope.ServiceProvider.GetRequiredService<ILlmPlayerService>();
                await tick.ExecuteAsync(CancellationToken.None);
            });
            return Results.Ok(new { ticked = "llm" });
        })
        .WithName("AdminTickLlm")
        .WithTags("Admin")
        .WithSummary("Run LLM player tick immediately")
        .WithDescription("Triggers the LLM player AI tick immediately instead of waiting for the cron schedule. Requires UseLlmPlayers feature flag.")
        .RequireAuthorization();


        app.MapPost("/admin/reset", async (
            ResetRequest request,
            HttpContext httpContext,
            UserManager<ApplicationUser> userManager,
            AppDbContext db,
            IMapService mapService,
            IHostApplicationLifetime hostLifetime,
            ILogger<AdminEndpoints> logger,
            CancellationToken ct
        ) =>
        {
            var guard = await AdminGuard(httpContext, userManager);
            if (guard is not null) return guard;

            var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var user = await userManager.FindByIdAsync(userId!);
            if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
                return Results.Problem("Wrong password", statusCode: 403);

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

                await db.SaveChangesAsync(ct);
            }

            logger.LogWarning("Soft database reset by admin — restarting");
            hostLifetime.StopApplication();
            return Results.Ok(new { reset = "soft", persistedUsers = keptUsers.Count, recreatedPlayers = nonAdminKept.Count });
        })
        .WithName("AdminResetSoft")
        .WithTags("Admin")
        .WithSummary("Soft reset game database")
        .WithDescription("Keeps registered (non-guest) Identity users, deletes all game data and guest accounts, creates a fresh starter village for each persisted user, then restarts the application for re-seeding. Requires admin password confirmation.")
        .RequireAuthorization();

        app.MapGet("/admin/config", ([FromServices] LlmPlayerConfig llmCfg, [FromServices] BarbarianOptions barbOptions) =>
            Results.Ok(new GameConfigResponse(
                GameSettings.TravelSpeedMultiplier,
                GameSettings.ResourcesProductionMultiplier,
                GameSettings.BuildSpeedMultiplier,
                GameSettings.TrainSpeedMultiplier,
                GameSettings.UpkeepMultiplier,
                BarbarianConfig.TargetPopulation,
                llmCfg.TickIntervalCron,
                barbOptions.TickIntervalCron))
        )
        .WithName("AdminGetConfig")
        .WithTags("Admin")
        .RequireAuthorization();

        app.MapPut("/admin/config", (
            UpdateGameConfigRequest request,
            [FromServices] LlmPlayerConfig llmCfg,
            [FromServices] BarbarianOptions barbOptions,
            [FromServices] IRecurringJobManager jobs) =>
        {
            if (request.TravelSpeedMultiplier.HasValue)
            {
                GameSettings.TravelSpeedMultiplier = request.TravelSpeedMultiplier.Value;
                GameSettings.ConfigVersion++;
            }
            if (request.ResourcesProductionMultiplier.HasValue)
            {
                GameSettings.ResourcesProductionMultiplier = request.ResourcesProductionMultiplier.Value;
                GameSettings.ConfigVersion++;
            }
            if (request.BuildSpeedMultiplier.HasValue)
            {
                GameSettings.BuildSpeedMultiplier = request.BuildSpeedMultiplier.Value;
                GameSettings.ConfigVersion++;
            }
            if (request.TrainSpeedMultiplier.HasValue)
            {
                GameSettings.TrainSpeedMultiplier = request.TrainSpeedMultiplier.Value;
                GameSettings.ConfigVersion++;
            }
            if (request.UpkeepMultiplier.HasValue)
            {
                GameSettings.UpkeepMultiplier = request.UpkeepMultiplier.Value;
                GameSettings.ConfigVersion++;
            }
            if (request.MaxBarbarianVillages.HasValue)
                BarbarianConfig.TargetPopulation = request.MaxBarbarianVillages.Value;
            if (request.LlmTickInterval is { Length: > 0 })
            {
                if (!TryParseCron(request.LlmTickInterval, out var error))
                    return Results.BadRequest(new { error });
                llmCfg.TickIntervalCron = request.LlmTickInterval;
                LlmPlayerJobScheduler.Register(jobs, llmCfg);
            }
            if (request.BarbarianTickInterval is { Length: > 0 })
            {
                if (!TryParseCron(request.BarbarianTickInterval, out var error))
                    return Results.BadRequest(new { error });
                barbOptions.TickIntervalCron = request.BarbarianTickInterval;
                BarbarianJobScheduler.Register(jobs, barbOptions);
            }
            return Results.Ok(new GameConfigResponse(
                GameSettings.TravelSpeedMultiplier,
                GameSettings.ResourcesProductionMultiplier,
                GameSettings.BuildSpeedMultiplier,
                GameSettings.TrainSpeedMultiplier,
                GameSettings.UpkeepMultiplier,
                BarbarianConfig.TargetPopulation,
                llmCfg.TickIntervalCron,
                barbOptions.TickIntervalCron));
        })
        .WithName("AdminUpdateConfig")
        .WithTags("Admin")
        .RequireAuthorization();
    }

    public record AddResourcesRequest(double Wood, double Clay, double Iron, double Beer);
    public record GameConfigResponse(float TravelSpeedMultiplier, float ResourcesProductionMultiplier, float BuildSpeedMultiplier, float TrainSpeedMultiplier, float UpkeepMultiplier, int MaxBarbarianVillages, string LlmTickInterval, string BarbarianTickInterval);
    public record UpdateGameConfigRequest(float? TravelSpeedMultiplier, float? ResourcesProductionMultiplier, float? BuildSpeedMultiplier, float? TrainSpeedMultiplier, float? UpkeepMultiplier, int? MaxBarbarianVillages, string? LlmTickInterval, string? BarbarianTickInterval);

    private static bool TryParseCron(string input, out string error)
    {
        try
        {
            CrontabSchedule.Parse(input);
            error = "";
            return true;
        }
        catch (Exception)
        {
            error = $"Invalid cron expression: {input}";
            return false;
        }
    }
}
