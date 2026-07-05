using TownManager.Domain.Entities;

namespace TownManager.Application.Interfaces;

public interface IPlayerRepository
{
    Task<Player?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Player?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<Player?> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task<bool> ExistsAsync(string username, CancellationToken ct = default);
    Task<string?> GetUserIdByPlayerIdAsync(Guid playerId, CancellationToken ct = default);
    Task<string?> GetUserIdByVillageIdAsync(Guid villageId, CancellationToken ct = default);
    void Add(Player player);
    Task<IReadOnlyList<Player>> GetAllPlayersWithTroopDataAsync(CancellationToken ct = default);
}