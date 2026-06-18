import { useEffect, useMemo, useRef, useState } from "react";
import { usePixiApp } from "./usePixiApp";
import { createApplication } from "../pixi/app";
import { MapScene } from "../pixi/scene/MapScene";
import { attachPan, attachZoom } from "../pixi/input";
import { useGameStateStore } from "../store/gameStateStore";
import { useMap } from "../api/hooks/useVillages";
import type { MapVillage } from "../api/types";
import { COLS, ROWS } from "../pixi/config";

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
  const setHoveredVillage = useGameStateStore((s) => s.setHoveredVillage);
  const setTargetVillage = useGameStateStore((s) => s.setTargetVillage);

  // Single fetch: pinned to the world center with a radius big enough to
  // cover the entire grid from any point. Empty deps so the query key is
  // stable for the lifetime of the renderer — no refetch on pan, on
  // active-village change, or on every store update.
  const mapRequest = useMemo(
    () => ({
      cords: { x: Math.floor(COLS / 2), y: Math.floor(ROWS / 2) },
      radius: Math.max(COLS, ROWS) * 2,
    }),
    [],
  );

  const { data: mapVillages } = useMap(mapRequest);

  // Merge the player's own villages (rich VillageDto) with the map
  // endpoint's lightweight PlayerVillageDto list.
  const allVillages = useMemo<MapVillage[]>(() => {
    const ownList: MapVillage[] = Object.values(villages).map((v) => ({
      ...v,
      kind: "own",
    }));
    const enemyList: MapVillage[] = (mapVillages ?? [])
      .filter((v) => !villages[v.id])
      .map((v) => ({ ...v, kind: "enemy" }));
    return [...ownList, ...enemyList];
  }, [mapVillages, villages]);

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
      allVillages,
      activeVillageId,
      // Own villages become the active selection; enemy villages set
      // the attack target.
      (village) => {
        if (village.kind === "own") {
          setActiveVillage(village.id);
        } else {
          setTargetVillage(village);
        }
      },
      (village) => setHoveredVillage(village),
    );
  }, [
    allVillages,
    activeVillageId,
    pixiReady,
    setActiveVillage,
    setHoveredVillage,
  ]);

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
