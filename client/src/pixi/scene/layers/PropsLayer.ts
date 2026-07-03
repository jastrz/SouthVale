import { Container } from "pixi.js";
import { createTerrainTile } from "../../entities/TerrainTile";
import { type TileData, gridSize } from "../../tileData";
import { treeTile, BUSH_TILES, TREE_TILES, pickTile } from "../../atlas";

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
        this.addChild(createTerrainTile(x, y, treeTile(...pickTile(pool))));
      }
    }
  }
}
