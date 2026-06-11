using MediatR;
using TownManager.Application.Common;

namespace TownManager.Application.Auth.Register;

public sealed record RegisterCommand(
    string Email, 
    string Password, 
    string Username) 
    : IRequest<Result<RegisterResponse>>;

public sealed record RegisterResponse(string AccessToken);
