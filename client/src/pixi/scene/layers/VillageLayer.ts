import { Container } from "pixi.js";
import type { MapVillage } from "../../../api/types";
import { createVillageMarker } from "../../entities/VillageMarker";

/**
 * Renders every village the client knows about on the world map, with
 * a single uniform API. Own and enemy markers live in the same layer;
 * the `MapVillage.kind` discriminator drives both the visual style
 * (see `createVillageMarker`) and the interactivity attached here.
 *
 * - `onSelect` fires on tap; consumers decide whether to make a
 *   village active, queue an attack, etc. (enemies simply don't get
 *   passed to `setActiveVillage` by the renderer).
 * - `onHover` fires on pointerover/pointerout with the village (or
 *   `null`) so the React side can show the right tooltip.
 */
export class VillageLayer extends Container {
  constructor() {
    super();
    this.label = "VillageLayer";
  }

  setVillages(
    villages: readonly MapVillage[],
    activeOwnId: string | null,
    onSelect?: (village: MapVillage) => void,
    onHover?: (village: MapVillage | null) => void,
  ): void {
    this.removeChildren().forEach((c) => c.destroy());
    for (const v of villages) {
      const isActive = v.kind === "own" && v.id === activeOwnId;
      const marker = createVillageMarker(v, isActive);
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
