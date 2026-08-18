using MediatR;
using TownManager.Application.Dtos;
using TownManager.Application.Leaderboard.Queries;

namespace TownManager.Api.Endpoints.Gameplay;

public class GetLeaderboardEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/gameplay/leaderboard", async (
            ISender sender,
            int? page,
            int? pageSize,
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new GetLeaderboardQuery(page ?? 1, pageSize ?? 20), ct);
            return result.ToHttpResponse();
        })
        .WithName("GetLeaderboard")
        .WithTags("Gameplay")
        .WithSummary("Get player leaderboard")
        .WithDescription("Returns players ranked by total troop count.")
        .RequireRateLimiting("Gameplay")
        .ProducesStandard<LeaderboardResultDto>();
    }
}
