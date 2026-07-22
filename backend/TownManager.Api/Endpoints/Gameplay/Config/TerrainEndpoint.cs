using TownManager.Application.Map.Services;

namespace TownManager.Api.Endpoints.Gameplay.Config;

public class TerrainEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/config/terrain", (IMapService mapService) =>
                Results.Content(mapService.GetTerrainJson(), "application/json"))
            .WithName("GetMapTiles")
            .WithTags("Gameplay")
            .WithSummary("Get map terrain tiles")
            .WithDescription("Returns terrain grid and decorations for the full map.")
            .RequireRateLimiting("Gameplay")
            .AllowAnonymous();
    }
}
