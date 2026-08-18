using MediatR;
using TownManager.Application.Dtos;
using TownManager.Application.Villages.Queries;

namespace TownManager.Api.Endpoints.Gameplay.Player;

public class GetPlayerVillagesEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/gameplay/player/{username}/villages", async (
            string username,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetPlayerVillagesQuery(username), ct);
            return result.ToHttpResponse();
        })
        .WithName("GetPlayerVillages")
        .WithTags("Gameplay")
        .WithSummary("Get villages owned by a player")
        .WithDescription("Returns all villages owned by the player with the given username.")
        .RequireRateLimiting("Gameplay")
        .AllowAnonymous()
        .ProducesStandard<IReadOnlyList<PlayerVillageDto>>(statusCodes: [StatusCodes.Status404NotFound]);
    }
}
