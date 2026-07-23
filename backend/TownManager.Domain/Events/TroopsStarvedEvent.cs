using MediatR;
using TownManager.Domain.Entities;

namespace TownManager.Domain.Events;

public record TroopsStarvedEvent(Guid PlayerId, string VillageName, Troops Starved) : INotification;

