import { Sprite, Texture } from "pixi.js";
import { TILE_SIZE } from "../config";

export function createTerrainTile(
  x: number,
  y: number,
  texture: Texture,
  xn = 0,
  yn = 0,
): Sprite {
  const sprite = new Sprite(texture);
  sprite.x = x * TILE_SIZE + xn;
  sprite.y = y * TILE_SIZE + yn;
  sprite.width = TILE_SIZE;
  sprite.height = TILE_SIZE;
  sprite.scale.set(TILE_SIZE / (64 - 1));
  return sprite;
}
