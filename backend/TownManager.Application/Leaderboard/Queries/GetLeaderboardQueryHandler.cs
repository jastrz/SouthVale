using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Dtos;
using TownManager.Application.Interfaces;
using TownManager.Domain.Enums;

namespace TownManager.Application.Leaderboard.Queries;

public class GetLeaderboardQueryHandler(IPlayerRepository playerRepo)
    : IRequestHandler<GetLeaderboardQuery, Result<LeaderboardResultDto>>
{
    public async Task<Result<LeaderboardResultDto>> Handle(
        GetLeaderboardQuery q, CancellationToken ct)
    {
        var players = await playerRepo.GetAllPlayersWithTroopDataAsync(ct);

        var scored = players
            .Select(p => new
            {
                p.Id,
                p.Username,
                Score = p.Villages.Sum(v => v.Troops.TotalCount +
                    v.TroopMovements
                        .Where(m => m.Status == MovementStatus.InFlight)
                        .Sum(m => m.Troops.TotalCount))
            })
            .OrderByDescending(x => x.Score)
            .ToList();

        var total = scored.Count;
        var skip = (q.Page - 1) * q.PageSize;

        var items = scored
            .Skip(skip)
            .Take(q.PageSize)
            .Select((x, i) => new LeaderboardEntryDto(
                x.Id, x.Username, x.Score, skip + i + 1))
            .ToList();

        return Result<LeaderboardResultDto>.Success(new LeaderboardResultDto(items, total));
    }
}
