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
        static int CountByType(TroopType type, Troops t) => type switch
        {
            TroopType.Swordsman => t.Get(TroopType.Swordsman),
            TroopType.Archer    => t.Get(TroopType.Archer),
            TroopType.Settler   => t.Get(TroopType.Settler),
            _ => 0,
        };

        var moving = movements
            .Where(m => m.Status == MovementStatus.InFlight)
            .Select(m => m.Troops);

        return ScoreConfig.Troop.Sum(kv =>
            (CountByType(kv.Key, garrison) + moving.Sum(t => CountByType(kv.Key, t))) * kv.Value);
    }

    private static int BuildingsScore(IEnumerable<Building> buildings) =>
        buildings.Sum(b =>
            ScoreConfig.Building.TryGetValue(b.Type, out var lv) &&
            lv.TryGetValue(b.Level, out var s) ? s : 0);
}