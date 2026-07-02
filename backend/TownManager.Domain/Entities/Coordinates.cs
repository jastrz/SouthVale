using System.Text.Json.Serialization;

namespace TownManager.Domain.Entities;

public class Coordinates : IEquatable<Coordinates>, IEquatable<(int x, int y)>
{
    public int X { get; }
    public int Y { get; }

    public Coordinates() { }

    [JsonConstructor]
    public Coordinates(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override string ToString() => $"({X}|{Y})";

    public bool Equals(Coordinates? other) => other is not null && (X == other.X && Y == other.Y);
    public bool Equals((int x, int y) other) => (X == other.x && Y == other.y);
    public override int GetHashCode() => HashCode.Combine(X, Y);

}