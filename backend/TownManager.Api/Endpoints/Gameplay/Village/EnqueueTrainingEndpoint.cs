using MediatR;
using TownManager.Api.Endpoints.Filters;
using TownManager.Application.Villages.Commands;

namespace TownManager.Api.Endpoints.Gameplay.Village;

public class EnqueueTrainingEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/gameplay/village/{villageId:guid}/train", async (
            Guid villageId, 
            EnqueueTrainingRequest request,
            ISender sender,
            CancellationToken ct
        ) =>
        {
            var result = await sender.Send(new CreateTrainOrderCommand(villageId, request.Orders), ct);
            return result.ToHttpResponse();
        })
        .WithName("QueueTraining")
        .WithTags("Gameplay")
        .WithSummary("Queue troop training")
        .WithDescription("Enqueues one or more troop training orders in the village's training queue.")
        .RequireRateLimiting("Gameplay")
        .RequireAuthorization()
        .AddEndpointFilter<VillageOwnershipFilter>()
        .ProducesStandard(statusCodes: [StatusCodes.Status400BadRequest, StatusCodes.Status401Unauthorized, StatusCodes.Status403Forbidden, StatusCodes.Status404NotFound]);
    }

    public record EnqueueTrainingRequest(IReadOnlyList<TroopEntry> Orders);
}