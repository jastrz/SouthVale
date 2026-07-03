import { Container } from "pixi.js";
import { createTerrainTile } from "../../entities/TerrainTile";
import { type TileData, gridSize } from "../../tileData";
import { treeTile, BUSH_TILES, TREE_TILES, pickTile } from "../../atlas";
import { PROPS_JITTER } from "../../config";

export class PropsLayer extends Container {
  constructor(grid: TileData[][]) {
    super();
    this.label = "PropsLayer";
    const { cols, rows } = gridSize(grid);
    for (let y = 0; y < rows; y++) {
      for (let x = 0; x < cols; x++) {
        const deco = grid[y]?.[x]?.decoration;
        if (!deco) continue;
        const pool =
          deco.kind === "tree"
            ? TREE_TILES
            : deco.kind === "bush"
              ? BUSH_TILES
              : null;
        if (!pool) continue;
        const jitterX = (Math.random() - 0.5) * 2 * PROPS_JITTER;
        const jitterY = (Math.random() - 0.5) * 2 * PROPS_JITTER;

        this.addChild(
          createTerrainTile(
            x,
            y,
            treeTile(...pickTile(pool)),
            jitterX,
            jitterY,
          ),
        );
      }
    }
  }
}
