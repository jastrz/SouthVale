using MediatR;
using TownManager.Api.Endpoints.Filters;
using TownManager.Application.Villages.Commands;

namespace TownManager.Api.Endpoints.Gameplay.Village;

public class RenameVillageEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPatch("/gameplay/village/{villageId:guid}/rename", async (
            Guid villageId,
            RenameVillageRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new RenameVillageCommand(villageId, request.Name), ct);
            return result.ToHttpResponse();
        })
        .WithName("RenameVillage")
        .WithTags("Gameplay")
        .WithSummary("Rename a village")
        .WithDescription("Renames a village owned by the authenticated player.")
        .RequireRateLimiting("Gameplay")
        .RequireAuthorization()
        .AddEndpointFilter<VillageOwnershipFilter>();
    }

    public record RenameVillageRequest(string Name);
}
