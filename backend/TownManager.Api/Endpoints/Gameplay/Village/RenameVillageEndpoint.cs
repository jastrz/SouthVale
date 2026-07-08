using System.Security.Claims;
using MediatR;
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
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Results.Problem(statusCode: 401, title: "Unauthorized");

            var result = await sender.Send(new RenameVillageCommand(villageId, request.Name, userId), ct);
            return result.ToHttpResponse();
        })
        .WithName("RenameVillage")
        .WithTags("Gameplay")
        .WithSummary("Rename a village")
        .WithDescription("Renames a village owned by the authenticated player.")
        .RequireRateLimiting("Gameplay")
        .RequireAuthorization();
    }

    public record RenameVillageRequest(string Name);
}
