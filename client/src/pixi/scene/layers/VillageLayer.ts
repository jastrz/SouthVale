import { Container } from "pixi.js";
import type { MapVillage } from "../../../api/types";
import { createVillageMarker } from "../../entities/VillageMarker";

/**
 * Renders every village the client knows about on the world map.
 */
export class VillageLayer extends Container {
  constructor() {
    super();
    this.label = "VillageLayer";
  }

  setVillages(
    villages: readonly MapVillage[],
    activeOwnId: string | null,
    targetId: string | null,
    onSelect?: (village: MapVillage) => void,
    onHover?: (village: MapVillage | null) => void,
  ): void {
    this.removeChildren().forEach((c) => c.destroy());
    const sorted = [...villages].sort(
      (a, b) => a.coordinates.y - b.coordinates.y,
    );
    for (const v of sorted) {
      const isActive = v.kind === "own" && v.id === activeOwnId;
      const isTarget = v.id === targetId;
      const marker = createVillageMarker(v, isActive, isTarget);
      marker.eventMode = "static";
      marker.cursor = "pointer";
      if (onSelect) {
        marker.on("pointertap", () => onSelect(v));
      }
      if (onHover) {
        marker.on("pointerover", () => onHover(v));
        marker.on("pointerout", () => onHover(null));
      }
      this.addChild(marker);
    }
  }
}
