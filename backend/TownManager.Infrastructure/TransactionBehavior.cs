using MediatR;
using Microsoft.EntityFrameworkCore;
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
        if (!typeof(Result).IsAssignableFrom(typeof(TResponse)))
            return await next();

        try
        {
            return await RunInTransaction();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Lost the race against a background tick: drop stale tracked
            // entities and run once more on fresh state.
            db.ChangeTracker.Clear();
            return await RunInTransaction();
        }

        async Task<TResponse> RunInTransaction()
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);

            var response = await next();

            if (response is Result { Succeeded: true })
                await tx.CommitAsync(ct);

            return response;
        }
    }
}
