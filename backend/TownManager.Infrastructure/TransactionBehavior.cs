using MediatR;
using TownManager.Application.Common;
using TownManager.Infrastructure.Persistence;

namespace TownManager.Infrastructure;

public class TransactionBehavior<TRequest, TResponse>(AppDbContext db)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : notnull
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (typeof(TResponse) != typeof(Result))
            return await next();

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        
        var response = await next();

        if (response is Result { Succeeded: true })
            await tx.CommitAsync(ct);

        return response;
    }
}
