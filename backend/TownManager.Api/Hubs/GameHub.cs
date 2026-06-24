using Microsoft.AspNetCore.SignalR;
using Serilog;
using TownManager.Application.Interfaces;

namespace TownManager.Api.Hubs;

public class GameHub(IConnectedUserTracker connectedUsers) : Hub
{
    public override Task OnConnectedAsync()
    {
        if (Context.UserIdentifier is { } userId)
        {
            connectedUsers.Add(userId);
            Log.Information("User {UserId} connected. User count: {Count}", userId, connectedUsers.GetConnectedUserIds().Count());
        }

        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.UserIdentifier is { } userId)
        {
            connectedUsers.Remove(userId);
            Log.Information("User {UserId} disconnected. User count: {Count}", userId, connectedUsers.GetConnectedUserIds().Count());
        }

        return base.OnDisconnectedAsync(exception);
    }
}
