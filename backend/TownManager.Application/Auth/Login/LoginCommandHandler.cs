using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Interfaces;

namespace TownManager.Application.Auth.Login;

public sealed class LoginCommandHandler(IAuthService authService)
    : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken ct)
    {
        var result = await authService.LoginAsync(request.Email, request.Password, ct);

        if (!result.Succeeded)
            return Result<LoginResponse>.Failure(result.Errors);

        return Result<LoginResponse>.Success(new LoginResponse(
            result.Value!.AccessToken, result.Value!.RefreshToken, result.Value!.Username));
    }
}
