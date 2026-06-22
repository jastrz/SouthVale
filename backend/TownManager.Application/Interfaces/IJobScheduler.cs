using System.Linq.Expressions;

namespace TownManager.Application.Interfaces;

public interface IJobScheduler
{
    string ScheduleBuildOrderResolution(Guid orderId, TimeSpan delay);
    string ScheduleTrainOrderResolution(Guid orderId, TimeSpan trainingTime);
    string ScheduleMovementResolution(Guid orderId, TimeSpan movementTime);
    bool DeleteJob(string jobId);
}