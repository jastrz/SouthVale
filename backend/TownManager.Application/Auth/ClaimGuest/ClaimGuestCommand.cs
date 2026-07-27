using MediatR;
using TownManager.Application.Common;

namespace TownManager.Application.Auth.ClaimGuest;

public sealed record ClaimGuestCommand(string UserId, string Email, string NewPassword, string Username) : IRequest<Result>;
