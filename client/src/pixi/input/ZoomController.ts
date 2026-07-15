import type { Container } from "pixi.js";
import { ZOOM } from "../config";

/**
 * Wheel-to-zoom (desktop) + pinch-to-zoom (touch), anchored on the
 * cursor/finger-center position. The world point under the cursor stays
 * under the cursor across scale changes.
 */
export type ZoomControllerOptions = {
  onZoom?: () => void;
};

export function attachZoom(
  canvas: HTMLCanvasElement,
  viewport: Container,
  options: ZoomControllerOptions = {},
): () => void {
  let pinchDist = 0;

  const dist = (a: Touch, b: Touch) =>
    Math.hypot(a.clientX - b.clientX, a.clientY - b.clientY);

  const zoomAt = (factor: number, cx: number, cy: number) => {
    const rect = canvas.getBoundingClientRect();
    const mouseX = cx - rect.left;
    const mouseY = cy - rect.top;
    const oldScale = viewport.scale.x;
    const newScale = Math.max(ZOOM.min, Math.min(ZOOM.max, oldScale * factor));
    if (newScale === oldScale) return;
    const worldX = (mouseX - viewport.x) / oldScale;
    const worldY = (mouseY - viewport.y) / oldScale;
    viewport.scale.set(newScale);
    viewport.x = mouseX - worldX * newScale;
    viewport.y = mouseY - worldY * newScale;
    options.onZoom?.();
  };

  const onWheel = (e: WheelEvent): void => {
    e.preventDefault();
    const factor = e.deltaY > 0 ? 1 - ZOOM.step : 1 + ZOOM.step;
    zoomAt(factor, e.clientX, e.clientY);
  };

  const onTouchStart = (e: TouchEvent): void => {
    if (e.touches.length === 2) {
      pinchDist = dist(e.touches[0], e.touches[1]);
    }
  };

  const onTouchMove = (e: TouchEvent): void => {
    if (e.touches.length !== 2 || pinchDist === 0) return;
    e.preventDefault();
    const d = dist(e.touches[0], e.touches[1]);
    const cx = (e.touches[0].clientX + e.touches[1].clientX) / 2;
    const cy = (e.touches[0].clientY + e.touches[1].clientY) / 2;
    const factor = d / pinchDist;
    zoomAt(factor, cx, cy);
    pinchDist = d;
  };

  const onTouchEnd = (): void => {
    pinchDist = 0;
  };

  canvas.addEventListener("wheel", onWheel, { passive: false });
  canvas.addEventListener("touchstart", onTouchStart);
  canvas.addEventListener("touchmove", onTouchMove, { passive: false });
  canvas.addEventListener("touchend", onTouchEnd);

  return () => {
    canvas.removeEventListener("wheel", onWheel);
    canvas.removeEventListener("touchstart", onTouchStart);
    canvas.removeEventListener("touchmove", onTouchMove);
    canvas.removeEventListener("touchend", onTouchEnd);
  };
}
