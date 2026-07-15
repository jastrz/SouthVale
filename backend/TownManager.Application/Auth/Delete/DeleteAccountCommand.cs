using MediatR;
using TownManager.Application.Common;

namespace TownManager.Application.Auth.Delete;

public sealed record DeleteAccountCommand(string UserId, string Password) : IRequest<Result<bool>>;