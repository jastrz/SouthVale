using MediatR;
using TownManager.Application.Common;

namespace TownManager.Application.Reports.Commands;

public record MarkReportReadCommand(string UserId, Guid ReportId) : IRequest<Result>;
