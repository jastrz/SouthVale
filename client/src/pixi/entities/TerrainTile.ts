import { Graphics } from "pixi.js";
import { COLORS, TILE } from "../config";

export function createTerrainTile(x: number, y: number): Graphics {
  return new Graphics()
    .rect(x * TILE, y * TILE, TILE - 1, TILE - 1)
    .fill(COLORS.tile);
}
