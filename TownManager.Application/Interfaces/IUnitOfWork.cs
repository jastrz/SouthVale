using Microsoft.EntityFrameworkCore.Storage;

namespace TownManager.Application.Interfaces;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct = default);
}