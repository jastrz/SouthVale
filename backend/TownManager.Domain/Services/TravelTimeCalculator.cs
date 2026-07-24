using TownManager.Domain.Config;
using TownManager.Domain.Entities;

namespace TownManager.Domain.Services;

public static class TravelTimeCalculator
{
    public static TimeSpan Calculate(Coordinates from, Coordinates to, int speed)
    {
        var distance = Math.Abs(from.X - to.X) + Math.Abs(from.Y - to.Y);
        return TimeSpan.FromHours((double)distance / speed / GameSettings.TravelSpeedMultiplier);
    }
}
