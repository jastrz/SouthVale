import { Container, Circle, Text } from "pixi.js";
import type { MapVillage } from "../../../api/types";
import { createVillageMarker } from "../../entities/VillageMarker";
import { ZOOM } from "../../config";

export class VillageLayer extends Container {
  private labels: Text[] = [];

  constructor() {
    super();
    this.label = "VillageLayer";
  }

  setVillages(
    villages: readonly MapVillage[],
    activeOwnId: string | null,
    targetId: string | null,
    onSelect?: (village: MapVillage, screenX: number, screenY: number) => void,
    onHover?: (village: MapVillage | null) => void,
  ): void {
    this.removeChildren().forEach((c) => c.destroy());
    this.labels = [];
    const sorted = [...villages].sort(
      (a, b) => a.coordinates.y - b.coordinates.y,
    );
    for (const v of sorted) {
      const isActive = v.kind === "own" && v.id === activeOwnId;
      const isTarget = v.id === targetId;
      const { marker, label } = createVillageMarker(v, isActive, isTarget);
      marker.eventMode = "static";
      marker.cursor = "pointer";
      marker.hitArea = new Circle(0, 0, 24);
      if (onSelect) {
        const onClick = onSelect;
        marker.on("pointertap", (e) => onClick(v, e.global.x, e.global.y));
      }
      if (onHover) {
        marker.on("pointerover", () => onHover(v));
        marker.on("pointerout", () => onHover(null));
      }
      this.addChild(marker);
      this.labels.push(label);
    }
  }

  setZoom(scale: number): void {
    const s = 1 / scale;
    const fadePoint = ZOOM.max / 3;
    const alpha = scale >= fadePoint ? 1 : Math.max(0.0, (scale - ZOOM.min) / (fadePoint - ZOOM.min));
    for (const lbl of this.labels) {
      lbl.scale.set(s);
      lbl.alpha = alpha;
    }
  }
}
