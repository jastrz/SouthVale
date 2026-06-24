using System.Collections.Concurrent;
using TownManager.Application.Interfaces;

namespace TownManager.Api.Hubs;

internal sealed class ConnectedUserTracker : IConnectedUserTracker
{
    private readonly ConcurrentDictionary<string, byte> _users = new();

    public IEnumerable<string> GetConnectedUserIds() => _users.Keys;
    public void Add(string userId) => _users.TryAdd(userId, 0);
    public void Remove(string userId) => _users.TryRemove(userId, out _);
}
