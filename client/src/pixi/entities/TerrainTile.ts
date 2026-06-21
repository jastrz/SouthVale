import { Sprite, Texture } from "pixi.js";
import { TILE } from "../config";

export function createTerrainTile(
  x: number,
  y: number,
  texture: Texture,
): Sprite {
  const sprite = new Sprite(texture);
  sprite.x = x * TILE;
  sprite.y = y * TILE;
  sprite.width = TILE;
  sprite.height = TILE;
  sprite.scale.set(TILE / (16 - 1));
  return sprite;
}
