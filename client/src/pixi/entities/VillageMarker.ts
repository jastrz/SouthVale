import { Graphics } from "pixi.js";
import type { MapVillage } from "../../api/types";
import { COLORS, TILE } from "../config";

/**
 * Renders a marker for any village on the world map. Own villages use the
 * green/active palette and respect the active highlight; enemy villages
 * use the red palette unconditionally. The factory is purely visual —
 * the layer attaches interactivity (`eventMode`, `cursor`, pointer
 * events) so callers can opt in or out per village kind.
 */
export function createVillageMarker(
  village: MapVillage,
  isActive: boolean,
): Graphics {
  const g = new Graphics();
  const isEnemy = village.kind === "enemy";

  if (isEnemy) {
    g.circle(0, 0, 10).fill({ color: COLORS.enemyVillageHalo, alpha: 0.35 });
    g.circle(0, 0, 6).fill(COLORS.enemyVillage);
  } else {
    g.circle(0, 0, 10).fill({
      color: isActive ? COLORS.villageHaloActive : COLORS.villageHalo,
      alpha: 0.35,
    });
    g.circle(0, 0, 6).fill(isActive ? COLORS.villageActive : COLORS.village);
  }

  g.x = village.coordinates.x * TILE + TILE / 2;
  g.y = village.coordinates.y * TILE + TILE / 2;
  return g;
}
