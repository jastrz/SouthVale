import { Container, Sprite, Text } from "pixi.js";
import type { TextStyleOptions } from "pixi.js";
import type { MapVillage } from "../../api/types";
import { TILE_SIZE, VILLAGE_SCALE, COLORS } from "../config";
import { villageTexture, arrowTexture } from "../atlas";
import { tween } from "../animation/";
import { useAuthStore } from "../../store/authStore";

const ARROW_AMPLITUDE = 5;
const ARROW_BOB_MS = 700;
const ARROW_OFFSET_Y = -40;
const ARROW_SCALE_PULSE = 0.15;
const ARROW_SCALE = { x: 0.06, y: 0.1 };
const VILLAGE_LABEL_CONTAINER_OFFSET_Y = -35;

/** Arrow marker hovering above the village */
function createArrow(color: number): Sprite {
  const sprite = new Sprite(arrowTexture());
  sprite.anchor.set(0.5, 1);
  sprite.scale.set(ARROW_SCALE.x, ARROW_SCALE.y);
  sprite.y = 0;
  sprite.eventMode = "none";
  sprite.tint = color;
  sprite.alpha = 0.8;
  const phase = (down: boolean) => {
    if (sprite.destroyed) return;
    tween({
      duration: ARROW_BOB_MS,
      onUpdate: (t) => {
        sprite.y = -(down ? t : 1 - t) * ARROW_AMPLITUDE;
        sprite.scale.set( ARROW_SCALE.x * (1 + (down ? t : 1 - t) * ARROW_SCALE_PULSE), ARROW_SCALE.y );
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
    align: "left",
    fontFamily: "georgia",
  };
  const playerText = new Text({
    text: playerName,
    style: { ...base, fontSize: 16, fontWeight: "bold", fill: playerNameColor(village, playerScore, myScore) },
    anchor: { x: 0.5, y: 0 },
    eventMode: "none",
    resolution: 4,
  });
  const nameText = new Text({
    text: name,
    style: { ...base, fontSize: 14, fill: 0xffffff },
    anchor: { x: 0.5, y: 0 },
    eventMode: "none",
    resolution: 4,
  });
  const popText = new Text({
    text: pop,
    style: { ...base, fontSize: 12, fill: 0xffffff },
    anchor: { x: 0.5, y: 0 },
    eventMode: "none",
    resolution: 4,
  });
  playerText.y = 0;
  nameText.y = 26;
  popText.y = 45;

  const container = new Container();
  container.addChild(playerText, nameText, popText);
  container.y = VILLAGE_LABEL_CONTAINER_OFFSET_Y;
  container.eventMode = "none";
  return container;
}

export function createVillageMarker(
  village: MapVillage,
  isActive: boolean,
  isTarget: boolean,
  playerScore: number | null,
  myScore: number,
): { marker: Container; arrows: Container; label: Container } {
  const isOwn = village.kind === "own";
  const sprite = new Sprite(
    villageTexture(
      "villageType" in village ? village.villageType : "Player",
      isOwn,
    ),
  );
  sprite.anchor.set(0.5);
  sprite.scale.set(VILLAGE_SCALE);

  const arrows = new Container();
  arrows.y = ARROW_OFFSET_Y;
  const label = createLabel(village, playerScore, myScore);

  const container = new Container();
  container.addChild(sprite, label, arrows);
  container.x = village.coordinates.x * TILE_SIZE + TILE_SIZE / 2;
  container.y = village.coordinates.y * TILE_SIZE + TILE_SIZE / 2;

  if (isActive) {
    arrows.addChild(createArrow(COLORS.arrowOwn));
  }

  if (isTarget) {
    arrows.addChild(createArrow(COLORS.arrowTarget));
  }

  return { marker: container, arrows, label };
}
