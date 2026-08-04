import { Container, Sprite, type Filter, type Texture } from "pixi.js";
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
import { treeSwayFilter, disposeTreeSwayFilter } from "../../shaders/treeSway";

export class PropsLayer extends Container {
  private readonly swayed = new Map<Sprite, Filter>();

  constructor(grid: TileData[][]) {
    super();
    this.label = "PropsLayer";
    this.sortableChildren = true;
    const { cols, rows } = gridSize(grid);
    const sway = (kind: "tree" | "bush") =>
      treeSwayFilter(
        kind === "tree" ? { amp: 4, freq: 2 } : { amp: 2, freq: 2 },
      );
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
      const filter = sway(kind);
      this.swayed.set(sprite, filter);
      if (TREE_SWAY_ENABLED) sprite.filters = [filter];
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

        // Big tree sprite is 2 tiles tall, anchored at the base (row y)
        const above = grid[y - 1]?.[x];
        const big =
          USE_BIG_TREES &&
          above !== undefined &&
          above.terrain === "grass" &&
          !above.decoration &&
          !above.occupied &&
          Math.random() < BIG_TREE_CHANCE
            ? pickBigTree()
            : null;
        if (big) {
          const sprite = new Sprite(bigTreeTile(...big));
          sprite.x = x * TILE_SIZE + jitterX;
          sprite.y = (y - 1) * TILE_SIZE + jitterY;
          sprite.width = TILE_SIZE;
          sprite.height = TILE_SIZE * 2;
          sprite.zIndex = y;
          const filter = sway("tree");
          this.swayed.set(sprite, filter);
          if (TREE_SWAY_ENABLED) sprite.filters = [filter];
          this.addChild(sprite);
        } else {
          place(x, y, treeTile(...pickTreeTile()), jitterX, jitterY, "tree");
        }
      }
    }
  }

  setSwayEnabled(enabled: boolean): void {
    for (const [sprite, filter] of this.swayed) {
      sprite.filters = enabled ? [filter] : [];
    }
  }

  override destroy(options?: Parameters<Container["destroy"]>[0]): void {
    for (const filter of this.swayed.values()) disposeTreeSwayFilter(filter);
    this.swayed.clear();
    super.destroy(options);
  }
}
