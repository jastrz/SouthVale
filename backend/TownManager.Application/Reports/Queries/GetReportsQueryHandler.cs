using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Dtos;
using TownManager.Application.Interfaces;

namespace TownManager.Application.Reports.Queries;

public class GetReportsQueryHandler(
    IPlayerRepository playerRepo,
    IReportRepository reportRepo)
    : IRequestHandler<GetReportsQuery, Result<ReportsResult>>
{
    public async Task<Result<ReportsResult>> Handle(GetReportsQuery q, CancellationToken ct)
    {
        var player = await playerRepo.GetByUserIdAsync(q.UserId, ct);
        if (player is null)
            return Result<ReportsResult>.Failure(["Player not found."], statusCode: 404);

        var reports = await reportRepo.GetByPlayerAsync(player.Id, ct: ct);
        var unread = await reportRepo.GetUnreadCountAsync(player.Id, ct);

        var dtos = reports.Select(r => new ReportDto(
            r.Id, r.Type, r.Title, r.Body, r.IsRead, r.CreatedAt)).ToList();

        return Result<ReportsResult>.Success(new ReportsResult(dtos, unread));
    }
}
