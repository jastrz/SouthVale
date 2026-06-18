namespace TownManager.Domain.Entities;

public class Coordinates : IEquatable<Coordinates>, IEquatable<(int x, int y)>
{
    public int X { get; set; }
    public int Y { get; set; }

    public Coordinates() { }

    public Coordinates(int x, int y)
    {
        X = x;
        Y = y;
    }

    public bool Equals(Coordinates? other) => other is not null && (X == other.X && Y == other.Y);
    public bool Equals((int x, int y) other) => (X == other.x && Y == other.y);
}