import { Sprite } from "pixi.js";
import { TILE } from "../config";
import { tile, ATLAS } from "../atlas";

export function createTerrainTile(x: number, y: number): Sprite {
  const sprite = new Sprite(tile(...ATLAS.GRASS_MC));
  sprite.x = x * TILE;
  sprite.y = y * TILE;
  sprite.width = TILE;
  sprite.height = TILE;
  sprite.scale.set(TILE / (16 - 1));
  return sprite;
}
