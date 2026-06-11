using Microsoft.EntityFrameworkCore.Storage;

namespace TownManager.Application.Interfaces;

public interface IUnitOfWork
{
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
    void MarkAsAdded<T>(T entity) where T : class;
}