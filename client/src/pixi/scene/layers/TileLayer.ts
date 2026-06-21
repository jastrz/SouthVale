import { Container } from "pixi.js";
import { COLS, ROWS } from "../../config";
import { createTerrainTile } from "../../entities/TerrainTile";
import { MAP_DATA } from "../../mapData";
import { autotile } from "../../autotile";
import { tile, ATLAS } from "../../atlas";

export class TileLayer extends Container {
  constructor() {
    super();
    this.label = "TileLayer";
    for (let x = 0; x < COLS; x++) {
      for (let y = 0; y < ROWS; y++) {
        const isWater = MAP_DATA[y]?.[x] === 1;
        const key = isWater ? ATLAS.WATER : autotile(MAP_DATA, x, y);
        this.addChild(createTerrainTile(x, y, tile(...key)));
      }
    }
  }
}
