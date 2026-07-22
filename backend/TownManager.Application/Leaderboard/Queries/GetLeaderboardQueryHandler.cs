using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Dtos;
using TownManager.Application.Interfaces;
using TownManager.Domain.Config;
using TownManager.Domain.Entities;
using TownManager.Domain.Entities.Villages;
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
                Score = p.Villages.Sum(v =>
                    TroopsScore(v.Troops, v.TroopMovements) +
                    BuildingsScore(v.Buildings))
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

    private static int TroopsScore(Troops garrison, ICollection<TroopMovement> movements)
    {
        var moving = movements
            .Where(m => m.Status == MovementStatus.InFlight)
            .Select(m => m.Troops);

        return Enum.GetValues<TroopType>().Sum(t =>
            (garrison.Get(t) + moving.Sum(m => m.Get(t))) * TroopsConfig.Get(t).Score);
    }

    private static int BuildingsScore(IEnumerable<Building> buildings) =>
        buildings.Sum(b =>
            BuildingConfig.Levels.TryGetValue(b.Type, out var levels) &&
            levels.FirstOrDefault(l => l.Level == b.Level) is { } cfg ? cfg.Score : 0);
}