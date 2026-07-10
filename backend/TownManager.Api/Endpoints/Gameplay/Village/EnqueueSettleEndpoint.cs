using MediatR;
using TownManager.Api.Endpoints.Filters;
using TownManager.Application.Villages.Commands;
using TownManager.Domain.Entities;

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
                new CreateSettleOrderCommand(villageId, request.Target),
                ct);

            return result.ToHttpResponse();
        })
        .WithName("QueueSettle")
        .WithTags("Gameplay")
        .WithSummary("Queue a settlement order")
        .WithDescription("Enqueues a settle order that will found a new village at the given map coordinates once processed.")
        .RequireRateLimiting("Gameplay")
        .RequireAuthorization()
        .AddEndpointFilter<VillageOwnershipFilter>();
    }

    public record EnqueueSettleRequest(Coordinates Target);
}
