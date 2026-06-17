import { Container } from "pixi.js";
import { COLS, ROWS } from "../../config";
import { createTerrainTile } from "../../entities/TerrainTile";

export class TileLayer extends Container {
  constructor() {
    super();
    this.label = "TileLayer";
    for (let x = 0; x < COLS; x++) {
      for (let y = 0; y < ROWS; y++) {
        this.addChild(createTerrainTile(x, y));
      }
    }
  }
}
