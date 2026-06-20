import { Container, Graphics, Sprite } from "pixi.js";
import type { MapVillage } from "../../api/types";
import { COLORS, TILE } from "../config";
import { tile, ATLAS } from "../atlas";

export function createVillageMarker(
  village: MapVillage,
  isActive: boolean,
): Container {
  const coord =
    village.kind === "enemy" ? ATLAS.VILLAGE_ENEMY : ATLAS.VILLAGE_OWN;
  const sprite = new Sprite(tile(coord[0], coord[1]));
  sprite.anchor.set(0.5);
  sprite.scale.set(2.0);

  const container = new Container();
  container.addChild(sprite);
  container.x = village.coordinates.x * TILE + TILE / 2;
  container.y = village.coordinates.y * TILE + TILE / 2;

  if (isActive) {
    const halo = new Graphics()
      .circle(0, 0, 22)
      .fill({ color: COLORS.villageHaloActive, alpha: 0.3 });
    container.addChild(halo);
  }

  return container;
}
