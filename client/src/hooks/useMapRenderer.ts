import { useEffect, useRef, useState } from "react";
import { usePixiApp } from "./usePixiApp";
import { createApplication } from "../pixi/app";
import { MapScene } from "../pixi/scene/MapScene";
import { attachPan, attachZoom } from "../pixi/input";
import { useGameStateStore } from "../store/gameStateStore";

/**
 * React↔Pixi bridge for the world map. Owns the Application and MapScene
 * lifecycle, attaches input controllers, and pushes store updates into the
 * scene.
 */
export function useMapRenderer(
  divRef: React.RefObject<HTMLDivElement | null>,
): void {
  const appRef = usePixiApp();
  const sceneRef = useRef<MapScene | null>(null);
  const [pixiReady, setPixiReady] = useState(false);

  const villages = useGameStateStore((s) => s.villages);
  const activeVillageId = useGameStateStore((s) => s.activeVillageId);
  const setActiveVillage = useGameStateStore((s) => s.setActiveVillage);

  useEffect(() => {
    if (!divRef.current) return;
    let mounted = true;
    let disposePan: (() => void) | null = null;
    let disposeZoom: (() => void) | null = null;

    (async () => {
      try {
        const app = await createApplication(divRef.current!);
        if (!mounted) {
          app.destroy(true);
          return;
        }
        appRef.current = app;

        const scene = new MapScene(app);
        sceneRef.current = scene;
        disposePan = attachPan(app.canvas, scene.root, {
          onDragStart: () => scene.cancelAnimation(),
          onPan: (x, y) => scene.setViewportPosition(x, y),
        });
        disposeZoom = attachZoom(app.canvas, scene.root, {
          onZoom: () => scene.clampViewport(),
        });
        setPixiReady(true);
      } catch (err) {
        console.error("Failed to initialize map renderer:", err);
      }
    })();

    return () => {
      mounted = false;
      disposePan?.();
      disposeZoom?.();
      setPixiReady(false);
      appRef.current?.destroy(true);
      appRef.current = null;
    };
  }, [divRef, appRef]);

  useEffect(() => {
    if (!pixiReady) return;
    sceneRef.current?.setVillages(
      Object.values(villages),
      activeVillageId,
      (v) => setActiveVillage(v.id),
    );
  }, [villages, activeVillageId, pixiReady, setActiveVillage]);

  // Pan to the active village whenever it changes. The ref guard keeps
  // subsequent store updates (resource ticks, etc.) from stealing the
  // camera when the user has panned away.
  const prevActiveIdRef = useRef<string | null>(null);
  useEffect(() => {
    if (!pixiReady) return;
    if (activeVillageId === prevActiveIdRef.current) return;
    prevActiveIdRef.current = activeVillageId;
    if (!activeVillageId) return;
    const village = villages[activeVillageId];
    if (!village) return;
    sceneRef.current?.centerOnVillage(village);
  }, [activeVillageId, pixiReady, villages]);
}
