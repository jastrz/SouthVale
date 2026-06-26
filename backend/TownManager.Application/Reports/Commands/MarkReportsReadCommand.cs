using MediatR;
using TownManager.Application.Common;

namespace TownManager.Application.Reports.Commands;

public record MarkReportsReadCommand(string UserId) : IRequest<Result>;
