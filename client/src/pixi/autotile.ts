import { ATLAS_GROUND } from "./atlas";
import type { TileData } from "./tileData";

/**
 * For a grass tile at (x, y), check the 8 neighbors and return
 * the correct ground edge/corner texture from ATLAS_GROUND so
 * ground auto-tiles around water.
 *
 * Returns [col, row] in the ground atlas.
 */
export function autotile(
  grid: TileData[][],
  x: number,
  y: number,
): readonly [number, number] {
  const rows = grid.length;
  const cols = grid[0].length;

  const isWater = (cx: number, cy: number) =>
    cy >= 0 &&
    cy < rows &&
    cx >= 0 &&
    cx < cols &&
    grid[cy][cx].terrain === "water";

  const n = isWater(x, y - 1);
  const s = isWater(x, y + 1);
  const w = isWater(x - 1, y);
  const e = isWater(x + 1, y);
  const nw = isWater(x - 1, y - 1);
  const ne = isWater(x + 1, y - 1);
  const sw = isWater(x - 1, y + 1);
  const se = isWater(x + 1, y + 1);

  // Outer corners
  if (n && w) return ATLAS_GROUND.GRASS_TL;
  if (n && e) return ATLAS_GROUND.GRASS_TR;
  if (s && w) return ATLAS_GROUND.GRASS_BL;
  if (s && e) return ATLAS_GROUND.GRASS_BR;

  // Edges
  if (n) return ATLAS_GROUND.GRASS_BM;
  if (s) return ATLAS_GROUND.GRASS_TC;
  if (w) return ATLAS_GROUND.GRASS_MR;
  if (e) return ATLAS_GROUND.GRASS_ML;

  // Inner corners
  if (nw && !n && !w) return ATLAS_GROUND.GRASS_IC_BR;
  if (ne && !n && !e) return ATLAS_GROUND.GRASS_IC_BL;
  if (sw && !s && !w) return ATLAS_GROUND.GRASS_IC_TR;
  if (se && !s && !e) return ATLAS_GROUND.GRASS_IC_TL;

  // weighted random pick
  const weights: { tile: readonly [number, number]; weight: number }[] = [
    { tile: ATLAS_GROUND.GRASS_MC1, weight: 40 },
    { tile: ATLAS_GROUND.GRASS_MC2, weight: 1 },
    { tile: ATLAS_GROUND.GRASS_MC3, weight: 1 },
  ];
  const total = weights.reduce((s, w) => s + w.weight, 0);
  let r = Math.random() * total;
  for (const { tile, weight } of weights) {
    r -= weight;
    if (r <= 0) return tile;
  }
  return weights[weights.length - 1].tile;
}
