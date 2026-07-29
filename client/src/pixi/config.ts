export const TILE_SIZE = 40;
export const VILLAGE_SCALE = 0.10;
export const PROPS_JITTER = 10;

export const ZOOM = {
  min: 0.5,
  max: 5,
  step: 0.1,
  default: 3,
} as const;

export const CAMERA = {
  // Base duration for a center-on-village pan, before distance scaling.
  centerBaseMs: 200,
  // Extra ms per pixel of pan distance; the pan also caps at centerMaxMs.
  centerPerPxMs: 0.5,
  centerMaxMs: 800,
} as const;

export const COLORS = {
  background: 0x1a1a2e,
  tile: 0x1a2a3a,
  village: 0x00ff00,
  villageActive: 0xffd700,
  villageHalo: 0x00aa00,
  villageHaloActive: 0xffffff,
  enemyVillage: 0xff3344,
  enemyVillageHalo: 0xaa2233,
  selectionFill: 0xffffff,
  targetFill: 0xff4444,
  targetHaloAlpha: 0.3,
  hoverOutline: 0xffffff,
  movement: {
    attackOutgoing: 0xff4400,
    attackIncoming: 0xff8800,
    transportOutgoing: 0x00ff00,
    transportIncoming: 0x00ff00,
    settle: 0x4488ff,
  },
} as const;

export const GRID = {
  hoverOutlineWidth: 2,
  hoverOutlineAlpha: 0.15,
  selectionFillAlpha: 0.2,
  clickDragThreshold: 5,
} as const;

export const MOVEMENT = {
  iconScale: .5,
  dash: 6,
  gap: 4,
  lineAlpha: 0.4,
  lineWidth: 2,
  arrowSize: 8,
  arrowAlpha: 0.6,
} as const;

export const PANEL = {
  minRem: 18,
  maxRem: 22,
  preferredVw: 18,
} as const;

export function panelWidth(): number {
  const minPx = PANEL.minRem * 16;
  const maxPx = PANEL.maxRem * 16;
  const preferredPx = window.innerWidth * (PANEL.preferredVw / 100);
  return Math.min(Math.max(preferredPx, minPx), maxPx);
}

export const PANEL_CLAMP = `clamp(${PANEL.minRem}rem,${PANEL.preferredVw}vw,${PANEL.maxRem}rem)`;
