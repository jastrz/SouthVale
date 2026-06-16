using MediatR;
using TownManager.Application.Villages.Commands;

namespace TownManager.Api.Endpoints.Gameplay.Village;

public class EnqueueSettleEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/gameplay/village/{villageId:guid}/settle", async (
            Guid villageId,
            EnqueueSettleRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new CreateSettleOrderCommand(villageId, request.TargetX, request.TargetY),
                ct);
            
            return result.Succeeded
                ? Results.Ok()
                : Results.BadRequest(result.Errors);
        })
        .WithName("QueueSettle")
        .WithTags("Gameplay")
        .WithSummary("Queue a settlement order")
        .WithDescription("Enqueues a settle order that will found a new village at the given map coordinates once processed.")
        .AllowAnonymous();
    }

    public record EnqueueSettleRequest(int TargetX, int TargetY);
}
