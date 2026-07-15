using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TownManager.Application.Players.Services;
using TownManager.Application.Villages.Services;
using TownManager.Domain.Config;
using TownManager.Domain.Entities;
using TownManager.Infrastructure.Identity;
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

            var added = new Resources(request.Wood, request.Clay, request.Iron, request.Crop);
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
                    effects.GranaryCapacity
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
    }

    public record AddResourcesRequest(double Wood, double Clay, double Iron, double Crop);
}
