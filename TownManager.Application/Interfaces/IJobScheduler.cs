using System.Linq.Expressions;

namespace TownManager.Application.Interfaces;

public interface IJobScheduler
{
    void ScheduleBuildOrderResolution(Guid orderId, TimeSpan delay);
    // void ScheduleTrainOrderResolution(Guid orderId, TimeSpan delay);
}