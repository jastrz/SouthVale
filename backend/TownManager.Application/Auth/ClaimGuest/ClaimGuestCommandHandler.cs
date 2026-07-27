using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Interfaces;

namespace TownManager.Application.Auth.ClaimGuest;

public sealed class ClaimGuestCommandHandler(
    IAuthService authService)
    : IRequestHandler<ClaimGuestCommand, Result>
{
    public async Task<Result> Handle(
        ClaimGuestCommand request, CancellationToken ct)
    {
        return await authService.ClaimGuestAsync(request.UserId, request.Email, request.NewPassword, request.Username, ct);
    }
}
