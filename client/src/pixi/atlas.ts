import { Assets, Texture, Rectangle } from "pixi.js";

const TILE = 16;
const GAP = 1;

const textures: Texture[][] = [];

/** Pre-extract all 120 tile textures into a [row][col] lookup. */
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

/** Get pre-extracted texture for tile at (row, col). */
export function tile(row: number, col: number): Texture {
  return textures[row]?.[col];
}

// Tile index to game-element mapping

export const ATLAS = {
  // Grass— 3×3 autotile block (water inside enviro)
  GRASS_TL: [3, 1] as const,
  GRASS_TC: [5, 2] as const,
  GRASS_TR: [3, 3] as const,
  GRASS_ML: [4, 3] as const,
  GRASS_MC: [4, 2] as const, // full grass tile
  GRASS_MR: [4, 1] as const,
  GRASS_BL: [5, 1] as const,
  GRASS_BC: [3, 2] as const,
  GRASS_BR: [5, 3] as const,

  // Inner corners — water touches the grass tile diagonally only
  GRASS_IC_TL: [3, 4] as const, // water at NW
  GRASS_IC_TR: [3, 5] as const, // water at NE
  GRASS_IC_BL: [4, 4] as const, // water at SW
  GRASS_IC_BR: [4, 5] as const, // water at SE

  WATER: [3, 6] as const,

  // Vegetation / forest
  TREES_SMALL: [3, 0] as const,
  TREE_SINGLE: [4, 0] as const,
  TREES_DOUBLE: [5, 0] as const,
  TREES_SINGLE2: [4, 6] as const,
  TREES_DOUBLE2: [5, 6] as const,

  // Village markers
  VILLAGE_OWN: [6, 6] as const,
  VILLAGE_SMALL: [7, 0] as const,
  VILLAGE_ENEMY: [6, 0] as const,
} as const;
