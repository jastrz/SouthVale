import { Container, Sprite, type Texture } from "pixi.js";
import { createTerrainTile } from "../../entities/TerrainTile";
import { type TileData, gridSize } from "../../tileData";
import {
  treeTile,
  bigTreeTile,
  pickTreeTile,
  pickBushTile,
  pickBigTree,
} from "../../atlas";
import {
  TILE_SIZE,
  PROPS_JITTER,
  TREE_SWAY_ENABLED,
  USE_BIG_TREES,
  BIG_TREE_CHANCE,
} from "../../config";
import { treeSwayFilter } from "../../shaders/treeSway";

export class PropsLayer extends Container {
  constructor(grid: TileData[][]) {
    super();
    this.label = "PropsLayer";
    this.sortableChildren = true;
    const { cols, rows } = gridSize(grid);
    const sway = (kind: "tree" | "bush") =>
      TREE_SWAY_ENABLED
        ? [
            treeSwayFilter(
              kind === "tree" ? { amp: 4, freq: 2 } : { amp: 2, freq: 2 },
            ),
          ]
        : undefined;
    const place = (
      x: number,
      y: number,
      tex: Texture,
      jx: number,
      jy: number,
      kind: "tree" | "bush",
    ) => {
      const sprite = createTerrainTile(x, y, tex, jx, jy);
      sprite.zIndex = y;
      sprite.filters = sway(kind);
      this.addChild(sprite);
    };
    for (let y = 0; y < rows; y++) {
      for (let x = 0; x < cols; x++) {
        const deco = grid[y]?.[x]?.decoration;
        if (!deco) continue;
        const jitterX = (Math.random() - 0.5) * 2 * PROPS_JITTER;
        const jitterY = (Math.random() - 0.5) * 2 * PROPS_JITTER;

        if (deco.kind === "bush") {
          place(x, y, treeTile(...pickBushTile()), jitterX, jitterY, "bush");
          continue;
        }

        const big =
          USE_BIG_TREES && Math.random() < BIG_TREE_CHANCE
            ? pickBigTree()
            : null;
        if (big) {
          const sprite = new Sprite(bigTreeTile(...big));
          sprite.x = x * TILE_SIZE + jitterX;
          sprite.y = y * TILE_SIZE + jitterY;
          sprite.width = TILE_SIZE;
          sprite.height = TILE_SIZE * 2;
          sprite.zIndex = y;
          sprite.filters = sway("tree");
          this.addChild(sprite);
        } else {
          place(x, y, treeTile(...pickTreeTile()), jitterX, jitterY, "tree");
        }
      }
    }
  }
}
