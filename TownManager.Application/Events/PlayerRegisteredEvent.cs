using MediatR;

namespace TownManager.Application.Events;

public record PlayerRegisteredEvent(Guid PlayerId, string Username) : INotification;