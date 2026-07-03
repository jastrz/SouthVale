import { Assets, Texture, Rectangle } from "pixi.js";

// Old tilesheet: 16px tiles, 1px gap, 12×10 grid
const TILE = 16;
const GAP = 1;
const textures: Texture[][] = [];

export async function loadAtlas(url: string): Promise<void> {
  const sheet = await Assets.load(url);
  const source = sheet.source;
  for (let row = 0; row < 10; row++) {
    textures[row] = [];
    for (let col = 0; col < 12; col++) {
      textures[row][col] = new Texture({
        source,
        frame: new Rectangle(
          col * (TILE + GAP),
          row * (TILE + GAP),
          TILE,
          TILE,
        ),
      });
    }
  }
}

/** Get pre-extracted texture at (row, col) from the old tilesheet. */
export function tile(row: number, col: number): Texture {
  return textures[row]?.[col];
}

// Ground atlas: 64px tiles, no gap, 6×7 grid. Tiles indexed as [col, row].
const GROUND_SIZE = 64;
const GROUND_COLS = 6;
const GROUND_ROWS = 7;
const groundTextures: Texture[][] = [];

export async function loadGroundAtlas(url: string): Promise<void> {
  const sheet = await Assets.load(url);
  const source = sheet.source;
  for (let row = 0; row < GROUND_ROWS; row++) {
    groundTextures[row] = [];
    for (let col = 0; col < GROUND_COLS; col++) {
      groundTextures[row][col] = new Texture({
        source,
        frame: new Rectangle(
          col * GROUND_SIZE,
          row * GROUND_SIZE,
          GROUND_SIZE,
          GROUND_SIZE,
        ),
      });
    }
  }
}

/** Get ground texture at [col, row]. */
export function groundTile(col: number, row: number): Texture {
  return groundTextures[row]?.[col];
}

// Trees atlas: 64px tiles, no gap, 19×4 grid.
const TREES_SIZE = 64;
const TREES_COLS = 19;
const TREES_ROWS = 4;
const treesTextures: Texture[][] = [];

export async function loadTreesAtlas(url: string): Promise<void> {
  const sheet = await Assets.load(url);
  const source = sheet.source;
  for (let row = 0; row < TREES_ROWS; row++) {
    treesTextures[row] = [];
    for (let col = 0; col < TREES_COLS; col++) {
      treesTextures[row][col] = new Texture({
        source,
        frame: new Rectangle(
          col * TREES_SIZE,
          row * TREES_SIZE,
          TREES_SIZE,
          TREES_SIZE,
        ),
      });
    }
  }
}

/** Get tree texture at [col, row]. */
export function treeTile(col: number, row: number): Texture {
  return treesTextures[row]?.[col];
}

// Tile index to game-element mapping — old tilesheet
export const ATLAS = {
  GRASS_TL: [3, 1] as const,
  GRASS_TC: [5, 2] as const,
  GRASS_TR: [3, 3] as const,
  GRASS_ML: [4, 3] as const,
  GRASS_MC: [4, 2] as const,
  GRASS_MR: [4, 1] as const,
  GRASS_BL: [5, 1] as const,
  GRASS_BC: [3, 2] as const,
  GRASS_BR: [5, 3] as const,
  GRASS_IC_TL: [3, 4] as const,
  GRASS_IC_TR: [3, 5] as const,
  GRASS_IC_BL: [4, 4] as const,
  GRASS_IC_BR: [4, 5] as const,
  WATER: [3, 6] as const,
  TREES_SMALL: [3, 0] as const,
  TREE_SINGLE: [4, 0] as const,
  TREES_DOUBLE: [5, 0] as const,
  TREES_SINGLE2: [4, 6] as const,
  TREES_DOUBLE2: [5, 6] as const,
  VILLAGE_OWN: [6, 6] as const,
  VILLAGE_SMALL: [7, 0] as const,
  VILLAGE_ENEMY: [6, 0] as const,
} as const;

// Ground tile mapping — [col, row] (x, y) in the 6×7 ground atlas
export const ATLAS_GROUND = {
  GRASS_TL: [3, 5] as const,
  GRASS_TC: [4, 0] as const,
  GRASS_TR: [4, 5] as const,
  GRASS_ML: [3, 1] as const,
  GRASS_MC1: [0, 6] as const,
  GRASS_MC2: [1, 6] as const,
  GRASS_MC3: [2, 6] as const,
  GRASS_MR: [5, 1] as const,
  GRASS_BL: [3, 6] as const,
  GRASS_BM: [4, 2] as const,
  GRASS_BR: [4, 6] as const,
  GRASS_IC_TL: [3, 0] as const,
  GRASS_IC_TR: [5, 0] as const,
  GRASS_IC_BL: [3, 2] as const,
  GRASS_IC_BR: [5, 2] as const,

  WATER: [4, 1] as const,
} as const;

export const ATLAS_TREES = {
  BUSH_RED: [0, 3],
  BUSH_YELLOW: [1, 3],
  BUSH_LIGHT_GREEN: [2, 3],
  BUSH_DARK_GREEN: [3, 3],

  TREE_RED: [10, 3],
  TREE_YELLOW: [11, 3],
  TREE_DARK_GREEN: [12, 3],
  TREE_LIGHT_GREEN: [13, 3],

  TREE2_DARK_GREEN: [18, 0],
  TREE2_LIGHT_GREEN: [18, 1],
} as const;

// Castle textures — loaded as full images
let playerVillage: Texture;
let enemyVillage: Texture;
let barbarianVillage: Texture;

export async function loadCastleTextures(): Promise<void> {
  const [player, enemy, barbarian] = await Promise.all([
    Assets.load("/villages/player_village.png"),
    Assets.load("/villages/enemy_village.png"),
    Assets.load("/villages/barbarian_village.png"),
  ]);
  playerVillage = new Texture({ source: player.source });
  enemyVillage = new Texture({ source: enemy.source });
  barbarianVillage = new Texture({ source: barbarian.source });
}

export function castleTexture(
  villageType: "Player" | "Barbarian",
  isOwn: boolean,
): Texture {
  if (isOwn) return playerVillage;
  return villageType === "Barbarian" ? barbarianVillage : enemyVillage;
}
