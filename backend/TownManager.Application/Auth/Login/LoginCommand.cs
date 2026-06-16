using MediatR;
using TownManager.Application.Common;

namespace TownManager.Application.Auth.Login;

public sealed record LoginCommand(string Email, string Password) 
    : IRequest<Result<LoginResponse>>;

public sealed record LoginResponse(string AccessToken);
