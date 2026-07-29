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
    return `${useAuthStore.getState().username}\n(${village.name} | ${pop} )`;
  }
  return `${village.playerName}\n(${village.name} | ${village.population} )`;
}

function createLabel(village: MapVillage): Text {
  const text = new Text({
    text: labelText(village),
    style: {
      fontSize: 11,
      fill: 0xffffff,
      stroke: { color: 0x111111, width: 1 },
      align: "center",
      fontFamily: "georgia",
    },
    anchor: { x: 0.5, y: 0 },
    eventMode: "none",
    resolution: 4
  });
  text.y = -50;
  return text;
}

export function createVillageMarker(
  village: MapVillage,
  isActive: boolean,
  isTarget: boolean,
): Container {
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
  container.addChild(createLabel(village));
  container.x = village.coordinates.x * TILE_SIZE + TILE_SIZE / 2;
  container.y = village.coordinates.y * TILE_SIZE + TILE_SIZE / 2;

  if (isActive) {
    container.addChild(createHalo(COLORS.villageHaloActive, 0.3));
  }

  if (isTarget) {
    container.addChild(createHalo(COLORS.targetFill, COLORS.targetHaloAlpha));
  }

  return container;
}
