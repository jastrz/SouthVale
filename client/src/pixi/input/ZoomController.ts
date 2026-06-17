import type { Container } from "pixi.js";
import { ZOOM } from "../config";

/**
 * Wheel-to-zoom, anchored on the cursor position. The world point under the
 * cursor stays under the cursor across scale changes;
 */
export type ZoomControllerOptions = {
  onZoom?: () => void;
};

export function attachZoom(
  canvas: HTMLCanvasElement,
  viewport: Container,
  options: ZoomControllerOptions = {},
): () => void {
  const onWheel = (e: WheelEvent): void => {
    e.preventDefault();

    const rect = canvas.getBoundingClientRect();
    const mouseX = e.clientX - rect.left;
    const mouseY = e.clientY - rect.top;

    const oldScale = viewport.scale.x;
    const factor = e.deltaY > 0 ? 1 - ZOOM.step : 1 + ZOOM.step;
    const newScale = Math.max(ZOOM.min, Math.min(ZOOM.max, oldScale * factor));
    if (newScale === oldScale) return;

    // World point under the cursor before the scale change.
    const worldX = (mouseX - viewport.x) / oldScale;
    const worldY = (mouseY - viewport.y) / oldScale;

    viewport.scale.set(newScale);

    // Re-anchor: keep the same world point under the cursor.
    viewport.x = mouseX - worldX * newScale;
    viewport.y = mouseY - worldY * newScale;

    options.onZoom?.();
  };

  canvas.addEventListener("wheel", onWheel, { passive: false });

  return () => {
    canvas.removeEventListener("wheel", onWheel);
  };
}
