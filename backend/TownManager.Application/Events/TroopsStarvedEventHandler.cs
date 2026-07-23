using MediatR;
using TownManager.Application.Interfaces;
using TownManager.Domain.Events;
using TownManager.Domain.Factories;

public class TroopsStarvedEventHandler(IReportRepository reportRepo) : INotificationHandler<TroopsStarvedEvent>
{
    public async Task Handle(TroopsStarvedEvent notification, CancellationToken cancellationToken)
    {
        var report = ReportFactory.StarvationReport(notification.PlayerId, notification.VillageName, notification.Starved);
        await reportRepo.AddAsync(report , cancellationToken);
    }
}