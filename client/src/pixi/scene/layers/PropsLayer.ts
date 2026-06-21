import { Container } from "pixi.js";
import { createTerrainTile } from "../../entities/TerrainTile";
import { type TileData, gridSize } from "../../tileData";
import { tile, ATLAS } from "../../atlas";

const TREE_KEYS: Record<number, readonly [number, number]> = {
  1: ATLAS.TREES_SMALL,
  2: ATLAS.TREE_SINGLE,
  3: ATLAS.TREES_DOUBLE,
  4: ATLAS.TREES_SINGLE2,
  5: ATLAS.TREES_DOUBLE2,
};

export class PropsLayer extends Container {
  constructor(grid: TileData[][]) {
    super();
    this.label = "PropsLayer";
    const { cols, rows } = gridSize(grid);
    for (let x = 0; x < cols; x++) {
      for (let y = 0; y < rows; y++) {
        const deco = grid[y]?.[x]?.decoration;
        if (!deco || deco.kind !== "tree") continue;
        const key = TREE_KEYS[deco.variant];
        if (!key) continue;
        this.addChild(createTerrainTile(x, y, tile(...key)));
      }
    }
  }
}
