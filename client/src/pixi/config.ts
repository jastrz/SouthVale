export const TILE_SIZE = 40;
export const VILLAGE_SCALE = 0.12;

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
  hoverOutline: 0xffffff,
} as const;

export const GRID = {
  hoverOutlineWidth: 2,
  hoverOutlineAlpha: 0.15,
  selectionFillAlpha: 0.2,
  clickDragThreshold: 5,
} as const;
