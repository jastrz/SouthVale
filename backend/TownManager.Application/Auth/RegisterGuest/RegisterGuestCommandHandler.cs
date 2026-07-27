using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Interfaces;

namespace TownManager.Application.Auth.RegisterGuest;

public sealed class RegisterGuestCommandHandler(
    IAuthService authService)
    : IRequestHandler<RegisterGuestCommand, Result<RegisterGuestResponse>>
{
    public async Task<Result<RegisterGuestResponse>> Handle(
        RegisterGuestCommand request, CancellationToken ct)
    {
        var result = await authService.RegisterGuestAsync(ct);

        if (!result.Succeeded)
            return Result<RegisterGuestResponse>.Failure(result.Errors);

        return Result<RegisterGuestResponse>.Success(new RegisterGuestResponse(
            result.Value!.AccessToken, result.Value!.RefreshToken,
            result.Value!.Username, result.Value!.Password!));
    }
}
