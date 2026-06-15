using MediatR;
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
            return result.Succeeded
                ? Results.Ok()
                : Results.BadRequest(result.Errors);
        })
        .WithName("QueueTraining")
        .WithTags("Gameplay")
        .AllowAnonymous();
    }
    
    public record EnqueueTrainingRequest(IReadOnlyList<TroopEntry> Orders);
}