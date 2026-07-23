using MediatR;
using TownManager.Api.Endpoints.Filters;
using TownManager.Application.Villages.Commands;
using TownManager.Domain.Enums;

namespace TownManager.Api.Endpoints.Gameplay.Village;

public class TradeResourcesEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/gameplay/village/{villageId:guid}/trade", async (
            Guid villageId,
            TradeRequest request,
            ISender sender,
            CancellationToken ct
        ) =>
        {
            var command = new TradeResourcesCommand(
                villageId,
                request.GiveType,
                request.GiveAmount,
                request.ReceiveType
            );

            var result = await sender.Send(command, ct);
            return result.ToHttpResponse();
        })
        .WithName("TradeResources")
        .WithTags("Gameplay")
        .WithSummary("Trade resources at the trading post")
        .WithDescription("Exchange one resource for another at the village's trading post. Rate depends on TradePost level.")
        .RequireRateLimiting("Gameplay")
        .RequireAuthorization()
        .AddEndpointFilter<VillageOwnershipFilter>();
    }

    public record TradeRequest(
        ResourceType GiveType,
        int GiveAmount,
        ResourceType ReceiveType
    );
}
