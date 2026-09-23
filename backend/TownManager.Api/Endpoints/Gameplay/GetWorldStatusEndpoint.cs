using MediatR;
using TownManager.Application.Dtos;
using TownManager.Application.World.Queries;

namespace TownManager.Api.Endpoints.Gameplay;

public class GetWorldStatusEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/gameplay/world", async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetWorldStatusQuery(), ct);
            return result.ToHttpResponse();
        })
        .WithName("GetWorldStatus")
        .WithTags("Gameplay")
        .WithSummary("Get world iteration status")
        .WithDescription("Returns the current world iteration, when it ends, and the previous iteration's top players.")
        .RequireRateLimiting("Gameplay")
        .ProducesStandard<WorldStatusDto>();
    }
}
