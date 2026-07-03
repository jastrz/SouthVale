import { Container } from "pixi.js";
import { createTerrainTile } from "../../entities/TerrainTile";
import { type TileData, gridSize } from "../../tileData";
import { treeTile, ATLAS_TREES } from "../../atlas";

const BUSH_TILES = [
  // ATLAS_TREES.BUSH_RED,
  ATLAS_TREES.BUSH_YELLOW,
  ATLAS_TREES.BUSH_LIGHT_GREEN,
  ATLAS_TREES.BUSH_DARK_GREEN,
] as const;

const TREE_TILES = [
  // ATLAS_TREES.TREE_RED,
  // ATLAS_TREES.TREE_YELLOW,
  ATLAS_TREES.TREE_DARK_GREEN,
  ATLAS_TREES.TREE_LIGHT_GREEN,
  ATLAS_TREES.TREE2_DARK_GREEN,
  ATLAS_TREES.TREE2_LIGHT_GREEN,
] as const;

function pick(pool: readonly (readonly [number, number])[]) {
  return pool[Math.floor(Math.random() * pool.length)];
}

export class PropsLayer extends Container {
  constructor(grid: TileData[][]) {
    super();
    this.label = "PropsLayer";
    const { cols, rows } = gridSize(grid);
    for (let x = 0; x < cols; x++) {
      for (let y = 0; y < rows; y++) {
        const deco = grid[y]?.[x]?.decoration;
        if (!deco) continue;
        const pool =
          deco.kind === "tree"
            ? TREE_TILES
            : deco.kind === "bush"
              ? BUSH_TILES
              : null;
        if (!pool) continue;
        this.addChild(createTerrainTile(x, y, treeTile(...pick(pool))));
      }
    }
  }
}
