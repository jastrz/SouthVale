using MediatR;
using TownManager.Application.Common;
using TownManager.Application.Interfaces;

namespace TownManager.Application.Auth.Delete;

public sealed class DeleteAccountCommandHandler(IAuthService authService)
    : IRequestHandler<DeleteAccountCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var result = await authService.DeleteAsync(request.UserId, request.Password, cancellationToken);

        if (!result.Succeeded)
            return Result<bool>.Failure(result.Errors);
        
        return Result<bool>.Success(true);
    }
}