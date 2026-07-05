using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Dtos;

namespace TownManager.Application.Leaderboard.Queries;

public record GetLeaderboardQuery(int Page = 1, int PageSize = 20)
    : IRequest<Result<LeaderboardResultDto>>;
