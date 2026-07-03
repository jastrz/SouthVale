import { Container, Graphics, Sprite } from "pixi.js";
import type { MapVillage } from "../../api/types";
import { COLORS, TILE_SIZE, VILLAGE_SCALE } from "../config";
import { castleTexture } from "../atlas";

export function createVillageMarker(
  village: MapVillage,
  isActive: boolean,
): Container {
  const isOwn = village.kind === "own";
  const sprite = new Sprite(
    castleTexture(
      "villageType" in village ? village.villageType : "Player",
      isOwn,
    ),
  );
  sprite.anchor.set(0.5);
  sprite.scale.set(VILLAGE_SCALE);

  const container = new Container();
  container.addChild(sprite);
  container.x = village.coordinates.x * TILE_SIZE + TILE_SIZE / 2;
  container.y = village.coordinates.y * TILE_SIZE + TILE_SIZE / 2;

  if (isActive) {
    const halo = new Graphics()
      .circle(0, 0, 22)
      .fill({ color: COLORS.villageHaloActive, alpha: 0.3 });
    container.addChild(halo);
  }

  return container;
}
