using MediatR;
using TownManager.Application.Dtos;
using TownManager.Application.Villages.Commands;

namespace TownManager.Api.Endpoints.Gameplay.Village;

public class SendTransportEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/gameplay/village/{villageId:guid}/transport", async (
            Guid villageId,
            SendTransportRequest request,
            ISender sender,
            CancellationToken ct
        ) =>
        {
            var command = new CreateTransportOrderCommand(
                villageId,
                request.TargetVillageId,
                request.Troops,
                request.Resources
            );

            var result = await sender.Send(command, ct);
            return result.ToHttpResponse();
        })
        .WithName("SendTransport")
        .WithTags("Gameplay")
        .WithSummary("Send a transport from a village")
        .WithDescription("Sends troops and resources from one of your villages to another.")
        .RequireAuthorization();
    }

    public record SendTransportRequest(
        IReadOnlyList<TroopEntry> Troops,
        Guid TargetVillageId,
        ResourcesDto Resources
    );
}
