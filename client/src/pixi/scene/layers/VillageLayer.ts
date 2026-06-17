import { Container } from "pixi.js";
import type { VillageDto } from "../../../api/types";
import { createVillageMarker } from "../../entities/VillageMarker";

export class VillageLayer extends Container {
  constructor() {
    super();
    this.label = "VillageLayer";
  }

  setVillages(
    villages: readonly VillageDto[],
    activeId: string | null,
    onSelect?: (village: VillageDto) => void,
  ): void {
    this.removeChildren().forEach((c) => c.destroy());
    for (const v of villages) {
      const marker = createVillageMarker(v, v.id === activeId);
      if (onSelect) {
        marker.on("pointertap", () => onSelect(v));
      }
      this.addChild(marker);
    }
  }
}
