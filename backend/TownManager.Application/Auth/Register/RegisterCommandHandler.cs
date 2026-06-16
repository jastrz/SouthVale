using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Interfaces;

namespace TownManager.Application.Auth.Register;

public sealed class RegisterCommandHandler(
    IAuthService authService)
    : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
{
    public async Task<Result<RegisterResponse>> Handle(
        RegisterCommand request, CancellationToken ct)
    {
        var result = await authService.RegisterAsync(
            request.Email, request.Password, request.Username, ct);

        if (!result.Succeeded)
            return Result<RegisterResponse>.Failure(result.Errors);

        return Result<RegisterResponse>.Success(new RegisterResponse(result.Value!));
    }
}
