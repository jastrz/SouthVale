using MediatR;
using TownManager.Application.Common;

namespace TownManager.Application.Auth.RegisterGuest;

public sealed record RegisterGuestCommand : IRequest<Result<RegisterGuestResponse>>;

public sealed record RegisterGuestResponse(string AccessToken, string RefreshToken, string Username, string Password);
