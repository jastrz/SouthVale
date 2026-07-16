import type { Container } from "pixi.js";

/**
 * Drag-to-pan. Attaches pointerdown/pointermove/pointerup listeners to the
 * canvas and translates `viewport` by the pointer delta.
 * Works for mouse, touch, and pen.
 */
export type PanControllerOptions = {
  onDragStart?: () => void;
  onPan?: (x: number, y: number) => void;
};

export function attachPan(
  canvas: HTMLCanvasElement,
  viewport: Container,
  options: PanControllerOptions = {},
): () => void {
  let dragging = false;
  let pointers = 0;
  let start = { x: 0, y: 0 };
  let origin = { x: 0, y: 0 };

  const xy = (e: PointerEvent) => ({ x: e.clientX, y: e.clientY });

  const onDown = (e: PointerEvent): void => {
    pointers++;
    if (pointers > 1) return;
    canvas.setPointerCapture(e.pointerId);
    dragging = true;
    start = xy(e);
    origin = { x: viewport.x, y: viewport.y };
    options.onDragStart?.();
  };

  const onMove = (e: PointerEvent): void => {
    if (!dragging || pointers > 1) return;
    const pos = xy(e);
    const newX = origin.x + (pos.x - start.x);
    const newY = origin.y + (pos.y - start.y);
    if (options.onPan) {
      options.onPan(newX, newY);
    } else {
      viewport.x = newX;
      viewport.y = newY;
    }
  };

  const onUp = (): void => {
    if (pointers > 1) dragging = false;
    pointers = Math.max(0, pointers - 1);
    if (pointers === 0) dragging = false;
  };

  canvas.addEventListener("pointerdown", onDown);
  canvas.addEventListener("pointermove", onMove);
  canvas.addEventListener("pointerup", onUp);
  canvas.addEventListener("pointercancel", onUp);

  return () => {
    canvas.removeEventListener("pointerdown", onDown);
    canvas.removeEventListener("pointermove", onMove);
    canvas.removeEventListener("pointerup", onUp);
    canvas.removeEventListener("pointercancel", onUp);
  };
}
