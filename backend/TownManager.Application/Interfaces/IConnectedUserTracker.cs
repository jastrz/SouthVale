namespace TownManager.Application.Interfaces;

public interface IConnectedUserTracker
{
    IEnumerable<string> GetConnectedUserIds();
    void Add(string userId);
    void Remove(string userId);
}
