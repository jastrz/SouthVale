using MediatR;
using TownManager.Application.Interfaces;
using TownManager.Domain.Events;
using TownManager.Domain.Factories;

public class TroopsStarvedEventHandler(
    IReportRepository reportRepo,
    IPlayerRepository playerRepo,
    IGameNotificationService gameNotificationService) : INotificationHandler<TroopsStarvedEvent>
{
    public async Task Handle(TroopsStarvedEvent notification, CancellationToken cancellationToken)
    {
        var report = ReportFactory.StarvationReport(notification.PlayerId, notification.VillageName, notification.Starved);
        var userId = await playerRepo.GetUserIdByPlayerIdAsync(notification.PlayerId, cancellationToken);
        if (userId is not null)
            await gameNotificationService.ReportCreatedAsync(userId, cancellationToken);
        await reportRepo.AddAsync(report, cancellationToken);
    }
}