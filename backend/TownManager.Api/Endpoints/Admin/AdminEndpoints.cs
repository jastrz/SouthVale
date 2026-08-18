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
    private static int _llmTickRunning;

    private static async Task<IResult?> AdminGuard(HttpContext httpContext, UserManager<ApplicationUser> userManager)
    {
        var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Unauthorized");
        var user = await userManager.FindByIdAsync(userId);
        if (user is null || !await userManager.IsInRoleAsync(user, "Admin"))
            return Results.Problem(statusCode: StatusCodes.Status403Forbidden, title: "Forbidden");
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
                .Select(v => new AdminVillageResponse(v.Id, v.Name, v.Coordinates))
                .ToListAsync(ct);

            return Results.Ok(villages);
        })
        .WithName("AdminListVillages")
        .WithTags("Admin")
        .WithSummary("List all villages")
        .WithDescription("Returns id, name, and coordinates for every village in the database.")
        .RequireAuthorization()
        .RequireRateLimiting("Gameplay")
        .ProducesStandard<IReadOnlyList<AdminVillageResponse>>(statusCodes: [StatusCodes.Status401Unauthorized, StatusCodes.Status403Forbidden]);

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
            if (village is null) return Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Village not found");

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
        .RequireAuthorization()
        .RequireRateLimiting("Gameplay")
        .ProducesStandard(statusCodes: [StatusCodes.Status401Unauthorized, StatusCodes.Status403Forbidden, StatusCodes.Status404NotFound]);

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

            // ExecuteUpdate bypasses the change tracker and concurrency token:
            // a background tick can write a village mid-loop without aborting this.
            foreach (var village in villages)
            {
                var cap = BuildingConfig.AggregateEffects(village.Buildings).WarehouseCapacity;
                await db.Villages
                    .Where(v => v.Id == village.Id)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(v => v.Resources.Wood, cap)
                        .SetProperty(v => v.Resources.Clay, cap)
                        .SetProperty(v => v.Resources.Iron, cap)
                        .SetProperty(v => v.Resources.Beer, cap)
                        .SetProperty(v => v.LastTickAt, DateTime.UtcNow)
                        .SetProperty(v => v.UpdatedAt, DateTime.UtcNow), ct);
            }

            return Results.Ok(new AdminFillResponse(villages.Count));
        })
        .WithName("AdminFullResources")
        .WithTags("Admin")
        .WithSummary("Set all villages to max resources")
        .WithDescription("Fills every village's warehouse and granary to full capacity. Returns count of villages filled.")
        .RequireAuthorization()
        .RequireRateLimiting("Gameplay")
        .ProducesStandard<AdminFillResponse>(statusCodes: [StatusCodes.Status401Unauthorized, StatusCodes.Status403Forbidden]);

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
                return Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: "Barbarians feature is disabled");

            await barbarianTick.ExecuteAsync(ct);
            return Results.Ok(new AdminTickResponse("barbarian"));
        })
        .WithName("AdminTickBarbarian")
        .WithTags("Admin")
        .WithSummary("Run barbarian tick immediately")
        .WithDescription("Triggers the barbarian AI tick immediately instead of waiting for the cron schedule. Requires UseBarbarians feature flag.")
        .RequireAuthorization()
        .RequireRateLimiting("Gameplay")
        .ProducesStandard<AdminTickResponse>(statusCodes: [StatusCodes.Status400BadRequest, StatusCodes.Status401Unauthorized, StatusCodes.Status403Forbidden]);
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
                return Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: "LLM players feature is disabled");

            // Don't fire llm tick job twice
            if (Interlocked.CompareExchange(ref _llmTickRunning, 1, 0) != 0)
                return Results.Problem(statusCode: StatusCodes.Status409Conflict, title: "LLM tick already in progress");

            // Check if hangfire job is executing
            var monitor = JobStorage.Current.GetMonitoringApi();
            var hangfireRunning =
                monitor.ProcessingJobs(0, 5000).Any(kv => kv.Value.Job.Type == typeof(LlmPlayerJob))
                || monitor.EnqueuedJobs("default", 0, 5000).Any(kv => kv.Value.Job.Type == typeof(LlmPlayerJob));
            if (hangfireRunning)
            {
                Interlocked.Exchange(ref _llmTickRunning, 0);
                return Results.Problem(statusCode: StatusCodes.Status409Conflict, title: "LLM tick already in progress (cron)");
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = scopeFactory.CreateScope();
                    var tick = scope.ServiceProvider.GetRequiredService<ILlmPlayerService>();
                    await tick.ExecuteAsync(CancellationToken.None);
                }
                finally
                {
                    Interlocked.Exchange(ref _llmTickRunning, 0);
                }
            });
            return Results.Ok(new AdminTickResponse("llm"));
        })
        .WithName("AdminTickLlm")
        .WithTags("Admin")
        .WithSummary("Run LLM player tick immediately")
        .WithDescription("Triggers the LLM player AI tick immediately instead of waiting for the cron schedule. Requires UseLlmPlayers feature flag.")
        .RequireAuthorization()
        .RequireRateLimiting("Gameplay")
        .ProducesStandard<AdminTickResponse>(statusCodes: [StatusCodes.Status400BadRequest, StatusCodes.Status401Unauthorized, StatusCodes.Status403Forbidden])
        .ProducesProblem(StatusCodes.Status409Conflict);


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
                return Results.Problem(statusCode: 403, title: "Wrong password");

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
            return Results.Ok(new AdminResetResponse("soft", keptUsers.Count, nonAdminKept.Count));
        })
        .WithName("AdminResetSoft")
        .WithTags("Admin")
        .WithSummary("Soft reset game database")
        .WithDescription("Keeps registered (non-guest) Identity users, deletes all game data and guest accounts, creates a fresh starter village for each persisted user, then restarts the application for re-seeding. Requires admin password confirmation.")
        .RequireAuthorization()
        .RequireRateLimiting("Gameplay")
        .ProducesStandard<AdminResetResponse>(statusCodes: [StatusCodes.Status401Unauthorized, StatusCodes.Status403Forbidden]);

        app.MapGet("/admin/config", async (
            HttpContext httpContext,
            UserManager<ApplicationUser> userManager,
            [FromServices] LlmPlayerConfig llmCfg,
            [FromServices] BarbarianOptions barbOptions) =>
        {
            var guard = await AdminGuard(httpContext, userManager);
            if (guard is not null) return guard;

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
        .WithName("AdminGetConfig")
        .WithTags("Admin")
        .RequireAuthorization()
        .RequireRateLimiting("Gameplay")
        .ProducesStandard<GameConfigResponse>(statusCodes: [StatusCodes.Status401Unauthorized, StatusCodes.Status403Forbidden]);

        app.MapPut("/admin/config", async (
            UpdateGameConfigRequest request,
            HttpContext httpContext,
            UserManager<ApplicationUser> userManager,
            [FromServices] LlmPlayerConfig llmCfg,
            [FromServices] BarbarianOptions barbOptions,
            [FromServices] IRecurringJobManager jobs) =>
        {
            var guard = await AdminGuard(httpContext, userManager);
            if (guard is not null) return guard;

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
                    return Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: error);
                llmCfg.TickIntervalCron = request.LlmTickInterval;
                LlmPlayerJobScheduler.Register(jobs, llmCfg);
            }
            if (request.BarbarianTickInterval is { Length: > 0 })
            {
                if (!TryParseCron(request.BarbarianTickInterval, out var error))
                    return Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: error);
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
        .RequireAuthorization()
        .RequireRateLimiting("Gameplay")
        .ProducesStandard<GameConfigResponse>(statusCodes: [StatusCodes.Status400BadRequest, StatusCodes.Status401Unauthorized, StatusCodes.Status403Forbidden]);
    }

    public record AddResourcesRequest(double Wood, double Clay, double Iron, double Beer);
    public record GameConfigResponse(float TravelSpeedMultiplier, float ResourcesProductionMultiplier, float BuildSpeedMultiplier, float TrainSpeedMultiplier, float UpkeepMultiplier, int MaxBarbarianVillages, string LlmTickInterval, string BarbarianTickInterval);
    public record UpdateGameConfigRequest(float? TravelSpeedMultiplier, float? ResourcesProductionMultiplier, float? BuildSpeedMultiplier, float? TrainSpeedMultiplier, float? UpkeepMultiplier, int? MaxBarbarianVillages, string? LlmTickInterval, string? BarbarianTickInterval);
    public record AdminVillageResponse(Guid Id, string Name, Coordinates Coordinates);
    public record AdminFillResponse(int Filled);
    public record AdminTickResponse(string Ticked);
    public record AdminResetResponse(string Reset, int PersistedUsers, int RecreatedPlayers);

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
