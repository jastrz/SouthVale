import { Container, Graphics, Sprite, Text } from "pixi.js";
import type { MapVillage } from "../../api/types";
import { COLORS, TILE_SIZE, VILLAGE_SCALE } from "../config";
import { villageTexture } from "../atlas";
import { useAuthStore } from "../../store/authStore";

function createHalo(color: number, alpha: number): Graphics {
  return new Graphics().circle(0, 0, 22).fill({ color, alpha });
}

function labelText(village: MapVillage): string {
  if (village.kind === "own") {
    const troops = village.troops;
    const pop = troops.swordsmen + troops.archers + troops.settlers + troops.dogs + troops.horsemen + troops.llamaRiders;
    return `${useAuthStore.getState().username}\n${village.name}\npop: ${pop}`;
  }
  return `${village.playerName}\n${village.name}\npop: ${village.population}`;
}

function createLabel(village: MapVillage): Text {
  const text = new Text({
    text: labelText(village),
    style: {
      fontSize: 16,
      fill: 0xffffff,
      stroke: { color: 0x111111, width: 2 },
      align: "left",
      fontFamily: "georgia",
    },
    anchor: { x: 0.5, y: 0 },
    eventMode: "none",
    resolution: 4
  });
  text.y = -40;
  return text;
}

export function createVillageMarker(
  village: MapVillage,
  isActive: boolean,
  isTarget: boolean,
): { marker: Container; label: Text } {
  const isOwn = village.kind === "own";
  const sprite = new Sprite(
    villageTexture(
      "villageType" in village ? village.villageType : "Player",
      isOwn,
    ),
  );
  sprite.anchor.set(0.5);
  sprite.scale.set(VILLAGE_SCALE);

  const container = new Container();
  container.addChild(sprite);
  const label = createLabel(village);
  container.addChild(label);
  container.x = village.coordinates.x * TILE_SIZE + TILE_SIZE / 2;
  container.y = village.coordinates.y * TILE_SIZE + TILE_SIZE / 2;

  if (isActive) {
    container.addChild(createHalo(COLORS.villageHaloActive, 0.3));
  }

  if (isTarget) {
    container.addChild(createHalo(COLORS.targetFill, COLORS.targetHaloAlpha));
  }

  return { marker: container, label };
}
