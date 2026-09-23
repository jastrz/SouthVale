using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Dtos;
using TownManager.Application.Interfaces;

namespace TownManager.Application.World.Queries;

public class GetWorldStatusQueryHandler(IWorldIterationRepository iterations, WorldResetOptions options)
    : IRequestHandler<GetWorldStatusQuery, Result<WorldStatusDto>>
{
    public async Task<Result<WorldStatusDto>> Handle(GetWorldStatusQuery q, CancellationToken ct)
    {
        var current = await iterations.GetCurrentAsync(ct);
        var finished = await iterations.GetFinishedAsync(ct);
        var previous = finished.Count > 0 ? finished[^1] : null;

        var best = finished
            .SelectMany(i => i.Winners.Select(w => new { i.Number, Winner = w }))
            .OrderByDescending(x => x.Winner.Score)
            .FirstOrDefault();

        var dto = new WorldStatusDto(
            current?.Number ?? 1,
            current?.StartedAt,
            current is null ? null : WorldResetSchedule.DueAt(current.StartedAt, options.IntervalDays),
            previous is null
                ? null
                : new PreviousWorldIterationDto(
                    previous.Number,
                    previous.EndedAt!.Value,
                    previous.Winners.Select(w => new WorldWinnerDto(w.Username, w.Score)).ToList()),
            best is null
                ? null
                : new BestEverWorldWinnerDto(best.Number, best.Winner.Username, best.Winner.Score));

        return Result<WorldStatusDto>.Success(dto);
    }
}
