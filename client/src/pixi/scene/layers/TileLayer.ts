import { Container } from "pixi.js";
import { createTerrainTile } from "../../entities/TerrainTile";
import { type TileData, gridSize } from "../../tileData";
import { autotile } from "../../autotile";
import { tile, ATLAS } from "../../atlas";

export class TileLayer extends Container {
  constructor(grid: TileData[][]) {
    super();
    this.label = "TileLayer";
    const { cols, rows } = gridSize(grid);
    for (let x = 0; x < cols; x++) {
      for (let y = 0; y < rows; y++) {
        const td = grid[y]?.[x];
        if (!td) continue;
        const key = td.terrain === "water" ? ATLAS.WATER : autotile(grid, x, y);
        this.addChild(createTerrainTile(x, y, tile(...key)));
      }
    }
  }
}
