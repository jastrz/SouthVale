using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MediatR;
using TownManager.Application.GameConfig.Queries;
using TownManager.Domain.Config;

namespace TownManager.Api.Endpoints.Gameplay;

public class GameConfigEndpoint : IEndpoint
{
    private static readonly string Etag = ComputeEtag();

    private static string ComputeEtag()
    {
        var data = JsonSerializer.Serialize(new { BuildingConfig.Levels, TroopsConfig.All });
        return $"\"{Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(data)))}\"";
    }

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/config", async (ISender sender, HttpContext context, CancellationToken ct) =>
        {
            if (context.Request.Headers.IfNoneMatch == Etag)
                return Results.StatusCode(304);

            var result = await sender.Send(new GetGameConfigQuery(), ct);
            context.Response.Headers.ETag = Etag;
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