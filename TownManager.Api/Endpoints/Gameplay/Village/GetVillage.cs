using MediatR;
using TownManager.Application.Villages.Queries;

namespace TownManager.Api.Endpoints.Gameplay.Village;

public class GetVillage : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/gameplay/village/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken ct
        ) =>
        {
            var result = await sender.Send(new GetVillageCommand(id), ct);
            return Results.Ok(result);
        });
    }
}