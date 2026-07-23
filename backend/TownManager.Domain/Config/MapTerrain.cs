using System.Text.Json;
using TownManager.Domain.Entities;

namespace TownManager.Domain.Config;

public static class MapTerrain
{
    private static string[]? _terrain;
    private static HashSet<(int, int)>? _blocked;
    private static int _cols, _rows;
    private static string? _rawJson;
    private static readonly Lock Lock = new();

    private static void EnsureLoaded()
    {
        if (_terrain is not null) return;
        lock (Lock)
        {
            if (_terrain is not null) return;
            var path = Path.Combine(AppContext.BaseDirectory, "data", "terrain.json");
            _rawJson = File.ReadAllText(path);
            using var doc = JsonDocument.Parse(_rawJson);
            var root = doc.RootElement;
            _cols = root.GetProperty("cols").GetInt32();
            _rows = root.GetProperty("rows").GetInt32();
            var t = root.GetProperty("terrain").GetString()!;
            _terrain = new string[_rows];
            for (int y = 0; y < _rows; y++)
                _terrain[y] = t.Substring(y * _cols, _cols);
            _blocked = [];
            if (root.TryGetProperty("decorations", out var decs))
            {
                foreach (var d in decs.EnumerateArray())
                    _blocked.Add((d[0].GetInt32(), d[1].GetInt32()));
            }
        }
    }

    public static bool IsWalkable(Coordinates coords)
    {
        EnsureLoaded();
        if (coords.X < 0 || coords.X >= _cols || coords.Y < 0 || coords.Y >= _rows)
            return false;
        return _terrain![coords.Y][coords.X] != 'W' && !_blocked!.Contains((coords.X, coords.Y));
    }

    public static string GetTerrainJson()
    {
        EnsureLoaded();
        return _rawJson!;
    }
}
