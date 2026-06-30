#define DEBUG_TIME

using TownManager.Domain.Entities;

namespace TownManager.Domain.Services;


public static class TravelTimeCalculator
{
    public static bool DebugMovementTime { get; set; } = true;

    public static TimeSpan Calculate(Coordinates from, Coordinates to, int speed)
    {
#if DEBUG_TIME
            return TimeSpan.FromSeconds(10);
#else

        var distance = Math.Abs(from.X - to.X) + Math.Abs(from.Y - to.Y);
        return TimeSpan.FromHours((double)distance / speed);
#endif
    }
}
