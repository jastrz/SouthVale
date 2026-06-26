using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Dtos;

namespace TownManager.Application.Reports.Queries;

public record GetReportsQuery(string UserId) : IRequest<Result<ReportsResult>>;

public record ReportsResult(IReadOnlyList<ReportDto> Reports, int UnreadCount);
