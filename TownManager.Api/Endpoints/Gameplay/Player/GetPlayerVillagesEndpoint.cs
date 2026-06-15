using MediatR;
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
            return result.Succeeded
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Errors);
        })
        .WithName("GetPlayerVillages")
        .WithTags("Gameplay")
        .AllowAnonymous();
    }
}
