export const TILE_SIZE = 40;
export const VILLAGE_SCALE = 0.1;
export const PROPS_JITTER = 10;
export let TREE_SWAY_ENABLED = load("tm-sway", true);
export let USE_BIG_TREES = load("tm-bigtrees", false);
export const BIG_TREE_CHANCE = 0.35;

function load(key: string, fallback: boolean): boolean {
  try {
    const v = localStorage.getItem(key);
    return v === null ? fallback : v === "1";
  } catch {
    return fallback;
  }
}

const PROPS_FLAGS_EVENT = "tm-props-flags";

function persist(key: string, v: boolean): void {
  try {
    localStorage.setItem(key, v ? "1" : "0");
  } catch {
    /* storage unavailable — session-only */
  }
}

export function setTreeSwayEnabled(v: boolean): void {
  TREE_SWAY_ENABLED = v;
  persist("tm-sway", v);
  window.dispatchEvent(new Event(PROPS_FLAGS_EVENT));
}

export function setUseBigTrees(v: boolean): void {
  USE_BIG_TREES = v;
  persist("tm-bigtrees", v);
  window.dispatchEvent(new Event(PROPS_FLAGS_EVENT));
}

export function onPropsFlagsChange(cb: () => void): () => void {
  window.addEventListener(PROPS_FLAGS_EVENT, cb);
  return () => window.removeEventListener(PROPS_FLAGS_EVENT, cb);
}

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
  arrowOwn: 0x86e276,
  arrowTarget: 0xff4444,
  labelPlayerStrong: 0xff4444,
  labelPlayerNeutral: 0xffffff,
  labelPlayerWeak: 0x86e276,
  labelPlayerBarbarian: 0xffd700,
  selectionFill: 0xffffff,
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
  lineAlpha: 0.6,
  lineWidth: 2,
  arrowSize: 8,
  arrowAlpha: 0.6,
} as const;

export const PANEL = {
  minRem: 20,
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
