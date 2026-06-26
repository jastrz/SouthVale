using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Interfaces;

namespace TownManager.Application.Reports.Commands;

public class MarkReportReadCommandHandler(
    IPlayerRepository playerRepo,
    IReportRepository reportRepo)
    : IRequestHandler<MarkReportReadCommand, Result>
{
    public async Task<Result> Handle(MarkReportReadCommand q, CancellationToken ct)
    {
        var player = await playerRepo.GetByUserIdAsync(q.UserId, ct);
        if (player is null)
            return Result.Failure(["Player not found."], statusCode: 404);

        await reportRepo.MarkAsReadAsync(q.ReportId, player.Id, ct);
        return Result.Success();
    }
}
