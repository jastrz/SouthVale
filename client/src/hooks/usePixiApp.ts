import { useRef } from "react";
import type { Application } from "pixi.js";

/**
 * Provides a stable ref to hold a Pixi Application. The consumer is
 * responsible for calling `app.destroy(true)` on cleanup — this hook
 * intentionally does not own the destroy, because the destroy must run
 * AFTER any scene/layer cleanup that touches `app.stage`, and React runs
 * useEffect cleanups in registration order.
 */
export function usePixiApp() {
  return useRef<Application | null>(null);
}
