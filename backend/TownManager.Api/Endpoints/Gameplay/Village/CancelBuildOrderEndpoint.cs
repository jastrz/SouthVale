using MediatR;
using TownManager.Application.Villages.Commands;

namespace TownManager.Api.Endpoints.Gameplay.Village;

public class CancelBuildOrderEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/gameplay/build/{orderId:guid}/cancel", async (
            Guid orderId,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CancelBuildOrderCommand(orderId), ct);
            return result.ToHttpResponse();
        })
        .WithName("CancelBuildOrder")
        .WithTags("Gameplay")
        .WithSummary("Cancel a build order")
        .WithDescription("Cancels a queued build order and refunds the resources.")
        .RequireAuthorization();
    }
}
