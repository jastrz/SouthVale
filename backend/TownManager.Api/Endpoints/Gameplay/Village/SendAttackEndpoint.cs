using MediatR;
using TownManager.Application.Villages.Commands;

namespace TownManager.Api.Endpoints.Gameplay.Village;

public class SendAttackEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/gameplay/village/{villageId:guid}/attack", async (
            Guid villageId,
            SendAttackRequest request,
            ISender sender,
            CancellationToken ct
        ) =>
        {
            var command = new CreateAttackOrderCommand(
                villageId,
                request.Troops,
                request.TargetVillageId
            );

            var result = await sender.Send(command, ct);
            
            return result.ToHttpResponse();
        })
        .WithName("SendAttack")
        .WithTags("Gameplay")
        .WithSummary("Send an attack from a village")
        .WithDescription("Creates an attack order that dispatches the specified troops from the source village to the target village.")
        .AllowAnonymous();
    }

    public record SendAttackRequest(IReadOnlyList<TroopEntry> Troops, Guid TargetVillageId);
}
