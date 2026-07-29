import { Container, Graphics, Rectangle, TilingSprite } from "pixi.js";
import type { Application, FederatedPointerEvent } from "pixi.js";
import type { Coordinates, MapVillage } from "../../api/types";
import { tween } from "../animation/";
import type { TweenHandle } from "../animation/";
import { CAMERA, TILE_SIZE, COLORS, GRID, ZOOM, panelWidth } from "../config";
import type { MovementDto } from "../../api/types";
import { TileLayer, PropsLayer, MovementLayer, VillageLayer } from "./layers";
import { type TileData, gridSize } from "../tileData";
import { groundTile, ATLAS_GROUND } from "../atlas";

type Bounds = { xMin: number; xMax: number; yMin: number; yMax: number };

const clamp = (v: number, min: number, max: number): number =>
  Math.max(min, Math.min(max, v));

/**
 * Top-level scene for the world map. Owns the root container, composes the
 * individual layers, and exposes a thin imperative API the React layer
 * can call to push store updates into Pixi.
 */
export class MapScene {
  readonly root = new Container();
  readonly tiles: TileLayer;
  readonly props: PropsLayer;
  readonly movements: MovementLayer;
  readonly villages: VillageLayer;
  readonly grid: TileData[][];
  private readonly app: Application;
  private cancelTween: TweenHandle | null = null;

  private readonly hoverHighlight = new Graphics();
  private readonly selectionFill = new Graphics();
  private onVillageHover: ((v: MapVillage | null) => void) | null = null;
  hoveredTile: { x: number; y: number } | null = null;
  selectedTile: { x: number; y: number } | null = null;

  constructor(
    app: Application,
    grid: TileData[][],
    onTileClick?: (x: number, y: number, tile: TileData | undefined) => void,
  ) {
    this.app = app;
    this.grid = grid;
    this.root.label = "MapRoot";
    this.tiles = new TileLayer(grid);
    this.props = new PropsLayer(grid);
    this.movements = new MovementLayer(app);
    this.villages = new VillageLayer();

    this.root.eventMode = "static";
    const { cols, rows } = gridSize(grid);
    this.root.hitArea = new Rectangle(0, 0, cols * TILE_SIZE, rows * TILE_SIZE);

    this.app.stage.addChild(this.root);
    this.createBackground(grid);
    this.root.addChild(this.tiles);
    this.root.addChild(this.props);
    this.root.addChild(this.selectionFill);
    this.root.addChild(this.hoverHighlight);
    this.root.addChild(this.villages);
    this.root.addChild(this.movements);

    let ptrDown = { x: 0, y: 0 };
    this.root.on("pointerdown", (e: FederatedPointerEvent) => {
      ptrDown = { x: e.global.x, y: e.global.y };
    });
    this.root.on("pointerup", (e: FederatedPointerEvent) => {
      if (e.target !== this.root) return;
      if (
        Math.hypot(e.global.x - ptrDown.x, e.global.y - ptrDown.y) >=
        GRID.clickDragThreshold
      )
        return;
      const local = e.getLocalPosition(this.root);
      const x = Math.floor(local.x / TILE_SIZE);
      const y = Math.floor(local.y / TILE_SIZE);
      const tile = this.grid[y]?.[x];
      this.selectedTile = { x, y };
      this.selectionFill.clear();
      this.selectionFill.rect(
        x * TILE_SIZE,
        y * TILE_SIZE,
        TILE_SIZE,
        TILE_SIZE,
      );
      this.selectionFill.fill({
        color: COLORS.selectionFill,
        alpha: GRID.selectionFillAlpha,
      });
      onTileClick?.(x, y, tile);
    });

    this.root.on("pointermove", (e: FederatedPointerEvent) => {
      if (e.target !== this.root) return;
      const local = e.getLocalPosition(this.root);
      const x = Math.floor(local.x / TILE_SIZE);
      const y = Math.floor(local.y / TILE_SIZE);
      this.hoveredTile = { x, y };
      this.hoverHighlight.clear();
      this.hoverHighlight.rect(
        x * TILE_SIZE,
        y * TILE_SIZE,
        TILE_SIZE,
        TILE_SIZE,
      );
      this.hoverHighlight.stroke({
        width: GRID.hoverOutlineWidth,
        color: COLORS.hoverOutline,
        alpha: GRID.hoverOutlineAlpha,
      });
    });

    this.root.on("pointerleave", () => {
      this.hoveredTile = null;
      this.hoverHighlight.clear();
      this.onVillageHover?.(null);
    });

    this.root.scale.set(ZOOM.default);

    this.center();
  }

  private createBackground(grid: TileData[][]): void {
    const { cols, rows } = gridSize(grid);
    const worldW = cols * TILE_SIZE;
    const worldH = rows * TILE_SIZE;
    const pad = Math.max(worldW, worldH, 100 * TILE_SIZE);
    const bg = new TilingSprite({
      texture: groundTile(...ATLAS_GROUND.GRASS_MC1),
      width: worldW + pad,
      height: worldH + pad,
      tileScale: { x: TILE_SIZE / 64, y: TILE_SIZE / 64 },
    });
    bg.position.set(-pad / 2, -pad / 2);
    this.root.addChildAt(bg, 0);
  }

  setVillages(
    villages: readonly MapVillage[],
    activeOwnId: string | null,
    targetId: string | null,
    onSelect?: (village: MapVillage, screenX: number, screenY: number) => void,
    onHover?: (village: MapVillage | null) => void,
  ): void {
    this.onVillageHover = onHover ?? null;
    this.villages.setVillages(
      villages,
      activeOwnId,
      targetId,
      onSelect,
      onHover
        ? (v) => {
            if (v) {
              this.hoveredTile = null;
              this.hoverHighlight.clear();
            }
            onHover(v);
          }
        : undefined,
    );
  }

  setMovements(
    movements: readonly MovementDto[],
    villageCoords: Record<string, Coordinates>,
    ownIds: Set<string>,
  ): void {
    this.movements.setMovements(movements, villageCoords, ownIds);
  }

  clearSelectedTile(): void {
    this.selectedTile = null;
    this.selectionFill.clear();
  }

  /**
   * Sets the viewport position, clamping to the valid range so the world
   * never fully leaves the canvas.
   */
  setViewportPosition(x: number, y: number): void {
    const b = this.computeViewportBounds();
    this.root.x = clamp(x, b.xMin, b.xMax);
    this.root.y = clamp(y, b.yMin, b.yMax);
  }

  /**
   * Re-clamps the current viewport position. Call after scale changes
   * (zoom) since the valid range shifts with scale.
   */
  clampViewport(): void {
    this.setViewportPosition(this.root.x, this.root.y);
  }

  /**
   * Pans the viewport so that the given village is centered on screen.
   * Preserves the current zoom level. The target is clamped to the valid
   * range, so villages at the world edge will land at the edge of the
   * canvas (visible but not geometrically centered) rather than dragging
   * the camera off-map.
   */
  centerOnVillage(
    village: { coordinates: Coordinates },
    options: { animate?: boolean; duration?: number } = {},
  ): void {
    const wx = village.coordinates.x * TILE_SIZE + TILE_SIZE / 2;
    const wy = village.coordinates.y * TILE_SIZE + TILE_SIZE / 2;
    const s = this.root.scale.x;
    const targetX = this.app.screen.width / 2 - wx * s;
    const targetY = this.app.screen.height / 2 - wy * s;
    const b = this.computeViewportBounds();
    const clampedX = clamp(targetX, b.xMin, b.xMax);
    const clampedY = clamp(targetY, b.yMin, b.yMax);

    if (options.animate === false) {
      this.cancelAnimation();
      this.root.x = clampedX;
      this.root.y = clampedY;
      return;
    }

    this.cancelAnimation();
    const startX = this.root.x;
    const startY = this.root.y;
    const dx = clampedX - startX;
    const dy = clampedY - startY;
    const distance = Math.hypot(dx, dy);
    const duration =
      options.duration ??
      Math.min(
        CAMERA.centerMaxMs,
        CAMERA.centerBaseMs + distance * CAMERA.centerPerPxMs,
      );

    // Skip the tween entirely for sub-pixel moves — running an animation
    // for 0.4px of travel just looks like a stutter.
    if (distance < 1) {
      this.root.x = clampedX;
      this.root.y = clampedY;
      return;
    }

    this.cancelTween = tween({
      duration,
      onUpdate: (t) => {
        if (!this.app.renderer) return;
        // Re-clamp each frame: if the user zooms mid-tween, the valid
        // range shifts and intermediate lerp values might leave it.
        const cb = this.computeViewportBounds();
        this.root.x = clamp(startX + dx * t, cb.xMin, cb.xMax);
        this.root.y = clamp(startY + dy * t, cb.yMin, cb.yMax);
      },
      onComplete: () => {
        if (this.root.destroyed) return;
        this.cancelTween = null;
      },
    });
  }

  /**
   * Cancels any in-flight camera tween. Called by PanController when the
   * user grabs the map so manual panning isn't fighting the animation.
   */
  cancelAnimation(): void {
    this.cancelTween?.();
    this.cancelTween = null;
  }

  destroy(): void {
    this.cancelAnimation();
    this.app.stage.removeChild(this.root);
    this.root.destroy({ children: true });
  }

  /**
   * Returns the valid camera range. When the world is larger than the
   * canvas, the range is `[canvas - world, 0]` so either edge of the
   * world can be aligned with the canvas edge. When the world fits, the
   * range collapses to the centered value.
   */
  private computeViewportBounds(): Bounds {
    const { cols, rows } = gridSize(this.grid);
    const worldW = cols * TILE_SIZE * this.root.scale.x;
    const worldH = rows * TILE_SIZE * this.root.scale.y;
    const cw = this.app.screen.width;
    const ch = this.app.screen.height;
    const pad = panelWidth() * 2;
    const cx = (cw - worldW) / 2;
    const cy = (ch - worldH) / 2;
    return {
      xMin: Math.min(cx, -(worldW - cw) - pad),
      xMax: Math.max(cx, pad),
      yMin: Math.min(cy, -(worldH - ch) - pad),
      yMax: Math.max(cy, pad),
    };
  }

  private center(): void {
    const { cols, rows } = gridSize(this.grid);
    this.root.x = this.app.screen.width / 2 - (cols * TILE_SIZE) / 2;
    this.root.y = this.app.screen.height / 2 - (rows * TILE_SIZE) / 2;
  }
}
