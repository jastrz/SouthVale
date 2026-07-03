import type { TileData } from "./tileData";

const FALLBACK_SIZE = 60;

type MapJson = {
  cols: number;
  rows: number;
  terrain: string; // row-major, G=grass W=water
  trees?: [number, number, number][]; // [x, y, variant] — legacy
  decorations?: [number, number, number, string][]; // [x, y, variant, kind]
};

/**
 * Load a map from a JSON file and convert to a TileData grid.
 * Falls back to an all-grass grid on fetch error.
 */
export async function loadMap(url: string): Promise<TileData[][]> {
  let json: MapJson;
  try {
    const res = await fetch(url);
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    json = await res.json();
  } catch {
    console.warn("map load failed, using all-grass fallback");
    return grassGrid(FALLBACK_SIZE, FALLBACK_SIZE);
  }

  const { cols, rows } = json;
  const grid: TileData[][] = [];
  for (let y = 0; y < rows; y++) {
    grid[y] = [];
    for (let x = 0; x < cols; x++) {
      const ch = json.terrain[y * cols + x];
      grid[y][x] = {
        terrain: ch === "W" ? "water" : "grass",
        decoration: null,
        occupied: false,
        villageId: null,
      };
    }
  }

  for (const entry of json.decorations ?? json.trees ?? []) {
    const [x, y, variant, kind] = entry;
    if (y < rows && x < cols) {
      grid[y][x].decoration = { kind: kind as "tree" | "bush", variant };
    }
  }

  return grid;
}

function grassGrid(cols: number, rows: number): TileData[][] {
  return Array.from({ length: rows }, () =>
    Array.from({ length: cols }, () => ({
      terrain: "grass" as const,
      decoration: null,
      occupied: false,
      villageId: null,
    })),
  );
}
