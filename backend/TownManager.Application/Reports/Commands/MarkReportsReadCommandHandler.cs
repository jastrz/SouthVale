using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Interfaces;

namespace TownManager.Application.Reports.Commands;

public class MarkReportsReadCommandHandler(
    IPlayerRepository playerRepo,
    IReportRepository reportRepo)
    : IRequestHandler<MarkReportsReadCommand, Result>
{
    public async Task<Result> Handle(MarkReportsReadCommand q, CancellationToken ct)
    {
        var player = await playerRepo.GetByUserIdAsync(q.UserId, ct);
        if (player is null)
            return Result.Failure(["Player not found."], statusCode: 404);

        await reportRepo.MarkAllAsReadAsync(player.Id, ct);
        return Result.Success();
    }
}
