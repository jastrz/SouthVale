import { Container, Sprite, Text } from "pixi.js";
import type { TextStyleOptions } from "pixi.js";
import type { MapVillage } from "../../api/types";
import { TILE_SIZE, VILLAGE_SCALE, COLORS } from "../config";
import { villageTexture, arrowTexture } from "../atlas";
import { tween } from "../animation/";
import { useAuthStore } from "../../store/authStore";

const ARROW_AMPLITUDE = 8;
const ARROW_BOB_MS = 700;
const ARROW_OFFSET_Y = -40;
export const ARROW_SCALE = 0.1;

/** Arrow marker hovering above the village */
function createArrow(color: number): Sprite {
  const sprite = new Sprite(arrowTexture());
  sprite.anchor.set(0.5, 1);
  sprite.scale.set(ARROW_SCALE);
  sprite.y = ARROW_OFFSET_Y;
  sprite.eventMode = "none";
  sprite.tint = color;
  sprite.alpha = 0.8;
  const phase = (down: boolean) => {
    if (sprite.destroyed) return;
    tween({
      duration: ARROW_BOB_MS,
      onUpdate: (t) => {
        sprite.y = ARROW_OFFSET_Y - (down ? t : 1 - t) * ARROW_AMPLITUDE;
      },
      onComplete: () => phase(!down),
    });
  };
  phase(false);
  return sprite;
}

function playerNameColor(village: MapVillage, playerScore: number | null, myScore: number): number {
  if ("villageType" in village && village.villageType === "Barbarian") {
    return COLORS.labelPlayerBarbarian;
  }
  if (playerScore == null || myScore <= 0) return COLORS.labelPlayerNeutral;
  const ratio = playerScore / myScore;
  if (ratio >= 1.5) return COLORS.labelPlayerStrong;
  if (ratio >= 0.5) return COLORS.labelPlayerNeutral;
  return COLORS.labelPlayerWeak;
}

function villageLabelText(village: MapVillage): { playerName: string; name: string; pop: string } {
  if (village.kind === "own") {
    const troops = village.troops;
    const pop = troops.swordsmen + troops.archers + troops.settlers + troops.dogs + troops.horsemen + troops.llamaRiders;
    return {
      playerName: useAuthStore.getState().username ?? "Player",
      name: village.name,
      pop: `pop: ${pop}`,
    };
  }
  return {
    playerName: village.playerName,
    name: village.name,
    pop: `pop: ${village.population}`,
  };
}

function createLabel(
  village: MapVillage,
  playerScore: number | null,
  myScore: number,
): Container {
  const { playerName, name, pop } = villageLabelText(village);
  const base: Partial<TextStyleOptions> = {
    stroke: { color: 0x111111, width: 2 },
    align: "left",
    fontFamily: "georgia",
  };
  const playerText = new Text({
    text: playerName,
    style: { ...base, fontSize: 18, fontWeight: "bold", fill: playerNameColor(village, playerScore, myScore) },
    anchor: { x: 0.5, y: 0 },
    eventMode: "none",
    resolution: 4,
  });
  const nameText = new Text({
    text: name,
    style: { ...base, fontSize: 16, fill: 0xffffff },
    anchor: { x: 0.5, y: 0 },
    eventMode: "none",
    resolution: 4,
  });
  const popText = new Text({
    text: pop,
    style: { ...base, fontSize: 14, fill: 0xffffff },
    anchor: { x: 0.5, y: 0 },
    eventMode: "none",
    resolution: 4,
  });
  playerText.y = 0;
  nameText.y = 26;
  popText.y = 45;

  const container = new Container();
  container.addChild(playerText, nameText, popText);
  container.y = -40;
  container.eventMode = "none";
  return container;
}

export function createVillageMarker(
  village: MapVillage,
  isActive: boolean,
  isTarget: boolean,
  playerScore: number | null,
  myScore: number,
): { marker: Container; label: Container; arrow: Sprite | null } {
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
  const label = createLabel(village, playerScore, myScore);
  container.addChild(label);
  container.x = village.coordinates.x * TILE_SIZE + TILE_SIZE / 2;
  container.y = village.coordinates.y * TILE_SIZE + TILE_SIZE / 2;

  let arrow: Sprite | null = null;
  if (isActive) {
    arrow = createArrow(COLORS.arrowOwn);
    container.addChild(arrow);
  }

  if (isTarget) {
    arrow = createArrow(COLORS.arrowTarget);
    container.addChild(arrow);
  }

  return { marker: container, label, arrow };
}
