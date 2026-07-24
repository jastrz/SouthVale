using MediatR;
using TownManager.Application.GameConfig.Queries;
using TownManager.Domain.Config;

namespace TownManager.Api.Endpoints.Gameplay.Config;

public class GameConfigEndpoint : IEndpoint
{
    private static readonly string _instanceId = Guid.NewGuid().ToString("N");

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/config", async (ISender sender, HttpContext context, CancellationToken ct) =>
        {
            var etag = $"\"{_instanceId}-{GameSettings.ConfigVersion}\"";
            if (context.Request.Headers.IfNoneMatch == etag)
                return Results.StatusCode(304);

            var result = await sender.Send(new GetGameConfigQuery(), ct);
            context.Response.Headers.ETag = etag;
            context.Response.Headers.CacheControl = "public, max-age=0, must-revalidate";
            return result.ToHttpResponse();
        })
        .WithName("GameConfig")
        .WithTags("Gameplay")
        .WithSummary("Get game configuration")
        .WithDescription("Returns all building and troop configuration data, including upgrade costs, effects, and stats.")
        .RequireRateLimiting("Gameplay")
        .AllowAnonymous();
    }
}