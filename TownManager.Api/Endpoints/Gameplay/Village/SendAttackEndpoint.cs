using MediatR;
using TownManager.Application.Villages.Commands;
using TownManager.Domain.Entities;

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
            
            return result.Succeeded
                ? Results.Ok()
                : Results.BadRequest(result.Errors);
        })
        .WithName("SendAttack")
        .WithTags("Gameplay")
        .AllowAnonymous();
    }

    public record SendAttackRequest(IReadOnlyList<TroopEntry> Troops, Guid TargetVillageId);
}
