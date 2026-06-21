import { ATLAS } from "./atlas";
import type { Tile } from "./mapData";

/**
 * For a grass tile at (x, y), check the 4 cardinal neighbors and return
 * the correct grass edge/corner texture so grass auto-tiles around water.
 *
 * Corners (two adjacent cardinal sides) → TL/TR/BL/BR
 * Edges (one side)                    → TC/BC/ML/MR
 * No water neighbors                  → MC (full grass)
 *
 */
export function autotile(
  grid: Tile[][],
  x: number,
  y: number,
): readonly [number, number] {
  const rows = grid.length;
  const cols = grid[0].length;
  const water = (cx: number, cy: number) =>
    cy >= 0 && cy < rows && cx >= 0 && cx < cols && grid[cy][cx] === 1;

  const n = water(x, y - 1);
  const s = water(x, y + 1);
  const w = water(x - 1, y);
  const e = water(x + 1, y);
  const nw = water(x - 1, y - 1);
  const ne = water(x + 1, y - 1);
  const sw = water(x - 1, y + 1);
  const se = water(x + 1, y + 1);

  // Outer corners
  if (n && w) return ATLAS.GRASS_TL;
  if (n && e) return ATLAS.GRASS_TR;
  if (s && w) return ATLAS.GRASS_BL;
  if (s && e) return ATLAS.GRASS_BR;

  // Edges
  if (n) return ATLAS.GRASS_BC;
  if (s) return ATLAS.GRASS_TC;
  if (w) return ATLAS.GRASS_MR;
  if (e) return ATLAS.GRASS_ML;

  // Inner corners
  if (nw && !n && !w) return ATLAS.GRASS_IC_BR;
  if (ne && !n && !e) return ATLAS.GRASS_IC_BL;
  if (sw && !s && !w) return ATLAS.GRASS_IC_TR;
  if (se && !s && !e) return ATLAS.GRASS_IC_TL;

  return ATLAS.GRASS_MC;
}
