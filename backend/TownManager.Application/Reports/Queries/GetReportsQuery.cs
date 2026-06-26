using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Dtos;

namespace TownManager.Application.Reports.Queries;

public record GetReportsQuery(string UserId, int Page = 1, int PageSize = 20) : IRequest<Result<ReportsResult>>;

public record ReportsResult(IReadOnlyList<ReportDto> Reports, int UnreadCount, int TotalCount);
