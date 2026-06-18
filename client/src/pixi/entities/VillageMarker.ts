import { Graphics } from "pixi.js";
import type { VillageDto } from "../../api/types";
import { COLORS, TILE } from "../config";

export function createVillageMarker(
  village: VillageDto,
  isActive: boolean,
): Graphics {
  const g = new Graphics();
  g.circle(0, 0, 10).fill({
    color: isActive ? COLORS.villageHaloActive : COLORS.villageHalo,
    alpha: 0.35,
  });
  g.circle(0, 0, 6).fill(isActive ? COLORS.villageActive : COLORS.village);
  g.x = village.coordinates.x * TILE + TILE / 2;
  g.y = village.coordinates.y * TILE + TILE / 2;
  g.eventMode = "static";
  g.cursor = "pointer";
  return g;
}
