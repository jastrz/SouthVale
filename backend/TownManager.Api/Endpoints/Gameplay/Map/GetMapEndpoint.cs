using MediatR;
using TownManager.Application.Map.Queries;
using TownManager.Domain.Entities;

namespace TownManager.Api.Endpoints.Gameplay.Map;

public class GetMapEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.Map("/gameplay/map", async (
                ISender sender,
                GetMapRequest request,
                CancellationToken ct ) =>
            {
                var result = await sender.Send(new GetMapQuery(request.Cords, request.Radius), ct);
                return result.ToHttpResponse();
            })
        .WithName("GetMap")
        .WithTags("Gameplay")
        .WithSummary("Get current map")
        .WithDescription("Returns all villages within requested radius from requested coordinates.")
        .RequireRateLimiting("Gameplay");
    }
}

public record GetMapRequest(Coordinates Cords, int Radius);