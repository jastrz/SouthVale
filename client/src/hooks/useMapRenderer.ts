import { useEffect, useMemo, useRef, useState } from "react";
import { usePixiApp } from "./usePixiApp";
import { createApplication } from "../pixi/app";
import { MapScene } from "../pixi/scene/MapScene";
import { loadMap } from "../pixi/mapLoader";
import { attachPan, attachZoom } from "../pixi/input";
import { useGameStateStore } from "../store/gameStateStore";
import { useMap } from "../api/hooks/useQueries";
import type { MapVillage } from "../api/types";

// Generous fetch radius for dev
const FETCH_RADIUS = 150;

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
  const activeVillageNonce = useGameStateStore((s) => s.activeVillageNonce);
  const setActiveVillage = useGameStateStore((s) => s.setActiveVillage);
  const setHoveredVillage = useGameStateStore((s) => s.setHoveredVillage);
  const setTargetVillage = useGameStateStore((s) => s.setTargetVillage);
  const setSelectedTile = useGameStateStore((s) => s.setSelectedTile);
  const selectedTile = useGameStateStore((s) => s.selectedTile);
  const targetVillage = useGameStateStore((s) => s.targetVillage);

  // Single fetch: pinned to the world center with a radius big enough to
  // cover the entire grid from any point. Empty deps so the query key is
  // stable for the lifetime of the renderer — no refetch on pan, on
  // active-village change, or on every store update.
  const mapRequest = useMemo(
    () => ({
      cords: { x: 0, y: 0 },
      radius: FETCH_RADIUS,
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
        const [app, grid] = await Promise.all([
          createApplication(divRef.current!),
          loadMap("/maps/default.json"),
        ]);
        if (!mounted) {
          app.destroy(true);
          return;
        }
        appRef.current = app;

        const scene = new MapScene(app, grid, (x, y) => {
          setSelectedTile({ x, y });
          setTargetVillage(null);
        });
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
  }, [divRef, appRef, setSelectedTile, setTargetVillage]);

  useEffect(() => {
    if (!pixiReady) return;
    sceneRef.current?.setVillages(
      allVillages,
      activeVillageId,
      targetVillage?.id ?? null,
      // Own villages become the active selection; enemy villages set
      // the attack target.
      (village) => {
        setSelectedTile(null);
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
    targetVillage,
    pixiReady,
    setActiveVillage,
    setHoveredVillage,
    setTargetVillage,
    setSelectedTile,
  ]);

  // Clear tile visual only when a village is freshly selected,
  // not when it's deselected (e.g. by clicking an empty tile).
  const prevActiveRef = useRef(activeVillageId);
  const prevTargetRef = useRef(targetVillage);
  useEffect(() => {
    if (!pixiReady) return;
    const gainedActive = activeVillageId && activeVillageId !== prevActiveRef.current;
    const gainedTarget = targetVillage && targetVillage !== prevTargetRef.current;
    prevActiveRef.current = activeVillageId;
    prevTargetRef.current = targetVillage;
    if (gainedActive || gainedTarget) {
      sceneRef.current?.clearSelectedTile();
    }
  }, [activeVillageId, targetVillage, pixiReady]);

  // Clear scene visual when tile selection is cleared from the panel
  useEffect(() => {
    if (!pixiReady) return;
    if (!selectedTile) {
      sceneRef.current?.clearSelectedTile();
    }
  }, [selectedTile, pixiReady]);

  // Pan to the active village whenever it changes.
  useEffect(() => {
    if (!pixiReady) return;
    if (!activeVillageId) return;
    const village = villages[activeVillageId];
    if (!village) return;
    sceneRef.current?.centerOnVillage(village);
  }, [activeVillageId, activeVillageNonce, pixiReady, villages]);
}
