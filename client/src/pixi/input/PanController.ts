import type { Container } from "pixi.js";

/**
 * Drag-to-pan. Attaches mousedown/mousemove/mouseup listeners to the canvas
 * and translates `viewport` by the mouse delta.
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
  let start = { x: 0, y: 0 };
  let origin = { x: 0, y: 0 };

  const onMouseDown = (e: MouseEvent): void => {
    dragging = true;
    start = { x: e.clientX, y: e.clientY };
    origin = { x: viewport.x, y: viewport.y };
    options.onDragStart?.();
  };

  const onMouseMove = (e: MouseEvent): void => {
    if (!dragging) return;
    const newX = origin.x + (e.clientX - start.x);
    const newY = origin.y + (e.clientY - start.y);
    if (options.onPan) {
      options.onPan(newX, newY);
    } else {
      viewport.x = newX;
      viewport.y = newY;
    }
  };

  const onMouseUp = (): void => {
    dragging = false;
  };

  canvas.addEventListener("mousedown", onMouseDown);
  canvas.addEventListener("mousemove", onMouseMove);
  canvas.addEventListener("mouseup", onMouseUp);

  return () => {
    canvas.removeEventListener("mousedown", onMouseDown);
    canvas.removeEventListener("mousemove", onMouseMove);
    canvas.removeEventListener("mouseup", onMouseUp);
  };
}
