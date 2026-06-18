import { Container } from "pixi.js";
import type { Application } from "pixi.js";
import type { VillageDto } from "../../api/types";
import { tween } from "../animation/";
import type { TweenHandle } from "../animation/";
import { CAMERA, COLS, ROWS, TILE } from "../config";
import { TileLayer, VillageLayer } from "./layers";

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
  readonly villages: VillageLayer;
  private readonly app: Application;
  private cancelTween: TweenHandle | null = null;

  constructor(app: Application) {
    this.app = app;
    this.root.label = "MapRoot";
    this.tiles = new TileLayer();
    this.villages = new VillageLayer();
    this.app.stage.addChild(this.root);
    this.root.addChild(this.tiles);
    this.root.addChild(this.villages);
    this.center();
  }

  setVillages(
    villages: readonly VillageDto[],
    activeId: string | null,
    onSelect?: (village: VillageDto) => void,
  ): void {
    this.villages.setVillages(villages, activeId, onSelect);
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
    village: VillageDto,
    options: { animate?: boolean; duration?: number } = {},
  ): void {
    const wx = village.coordinates.x * TILE + TILE / 2;
    const wy = village.coordinates.y * TILE + TILE / 2;
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
        if (this.root.destroyed) return;
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
    const worldW = COLS * TILE * this.root.scale.x;
    const worldH = ROWS * TILE * this.root.scale.y;
    const canvasW = this.app.screen.width;
    const canvasH = this.app.screen.height;
    return {
      xMin: worldW >= canvasW ? canvasW - worldW : (canvasW - worldW) / 2,
      xMax: worldW >= canvasW ? 0 : (canvasW - worldW) / 2,
      yMin: worldH >= canvasH ? canvasH - worldH : (canvasH - worldH) / 2,
      yMax: worldH >= canvasH ? 0 : (canvasH - worldH) / 2,
    };
  }

  private center(): void {
    this.root.x = this.app.screen.width / 2 - (COLS * TILE) / 2;
    this.root.y = this.app.screen.height / 2 - (ROWS * TILE) / 2;
  }
}
