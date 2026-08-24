using System.Security.Claims;
using MediatR;
using TownManager.Application.Dtos;
using TownManager.Application.Players.Queries;

namespace TownManager.Api.Endpoints.Gameplay.Player;

public class GetEmpireEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/gameplay/me/empire", async (
            ISender sender,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Results.Problem(statusCode: 401, title: "Unauthorized");

            var result = await sender.Send(new GetEmpireQuery(userId), ct);
            return result.ToHttpResponse();
        })
        .WithName("GetEmpire")
        .WithTags("Gameplay")
        .WithSummary("Get empire-wide stats")
        .WithDescription("Returns player-wide values derived from all villages.")
        .RequireRateLimiting("Gameplay")
        .RequireAuthorization()
        .ProducesStandard<EmpireDto>(statusCodes: [StatusCodes.Status401Unauthorized, StatusCodes.Status404NotFound]);
    }
}