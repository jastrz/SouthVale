import { Container } from "pixi.js";
import { createTerrainTile } from "../../entities/TerrainTile";
import { type TileData, gridSize } from "../../tileData";
import { autotile } from "../../autotile";
import { groundTile, ATLAS_GROUND } from "../../atlas";

export class TileLayer extends Container {
  constructor(grid: TileData[][]) {
    super();
    this.label = "TileLayer";
    const { cols, rows } = gridSize(grid);
    for (let y = 0; y < rows; y++) {
      for (let x = 0; x < cols; x++) {
        const td = grid[y]?.[x];
        if (!td) continue;
        if (td.terrain === "water") {
          this.addChild(
            createTerrainTile(x, y, groundTile(...ATLAS_GROUND.WATER)),
          );
        } else {
          this.addChild(
            createTerrainTile(x, y, groundTile(...autotile(grid, x, y))),
          );
        }
      }
    }
  }
}
