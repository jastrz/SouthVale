export const TILE = 40;
export const COLS = 50;
export const ROWS = 50;

export const ZOOM = {
  min: 0.5,
  max: 3,
  step: 0.1,
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
  villageHaloActive: 0xffd700,
} as const;
