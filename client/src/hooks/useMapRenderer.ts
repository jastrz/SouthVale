import { useEffect, useMemo, useRef, useState } from "react";
import { usePixiApp } from "./usePixiApp";
import { createApplication } from "../pixi/app";
import { MapScene } from "../pixi/scene/MapScene";
import { loadMap } from "../pixi/mapLoader";
import { attachPan, attachZoom } from "../pixi/input";
import { useGameStateStore } from "../store/gameStateStore";
import { useMap, useMovements, useLeaderboard } from "../api/hooks/useQueries";
import { useAuthStore } from "../store/authStore";
import type { MapVillage, Coordinates } from "../api/types";

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
  const setTargetVillagePos = useGameStateStore((s) => s.setTargetVillagePos);
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

  const { data: movements } = useMovements();

  // Full leaderboard: playerId -> score, for label coloring on the map.
  const { data: leaderboard } = useLeaderboard(1, 1000);
  const scores = useMemo(
    () => Object.fromEntries((leaderboard?.items ?? []).map((e) => [e.playerId, e.score])),
    [leaderboard],
  );
  const myScore = useMemo(
    () => leaderboard?.items.find((e) => e.username === useAuthStore.getState().username)?.score ?? 0,
    [leaderboard],
  );

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

  // Build coordinate lookup from all known villages
  const villageCoords = useMemo(() => {
    const map: Record<string, Coordinates> = {};
    for (const v of allVillages) {
      map[v.id] = v.coordinates;
    }
    return map;
  }, [allVillages]);

  // Set of own village IDs for direction check
  const ownIds = useMemo(() => new Set(Object.keys(villages)), [villages]);

  useEffect(() => {
    if (!divRef.current) return;
    let mounted = true;
    let disposePan: (() => void) | null = null;
    let disposeZoom: (() => void) | null = null;

    (async () => {
      try {
        const [app, grid] = await Promise.all([
          createApplication(divRef.current!),
          loadMap(`${import.meta.env.VITE_API_URL ?? ""}/config/terrain`),
        ]);
        if (!mounted) {
          app.destroy(true);
          return;
        }
        appRef.current = app;

        const scene = new MapScene(app, grid, (x, y, tile) => {
          setSelectedTile(tile ? { x, y, tile } : null);
          setTargetVillage(null);
          setTargetVillagePos(null);
        });
        sceneRef.current = scene;
        disposePan = attachPan(app.canvas, scene.root, {
          onDragStart: () => scene.cancelAnimation(),
          onPan: (x, y) => scene.setViewportPosition(x, y),
        });
        disposeZoom = attachZoom(app.canvas, scene.root, {
          onZoom: () => {
            scene.clampViewport();
            scene.villages.setZoom(scene.root.scale.x);
          },
        });
        scene.villages.setZoom(scene.root.scale.x);
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
  }, [divRef, appRef, setSelectedTile, setTargetVillage, setTargetVillagePos]);

  useEffect(() => {
    if (!pixiReady) return;
    const scene = sceneRef.current;
    if (!scene) return;
    scene.setVillages(
      allVillages,
      activeVillageId,
      targetVillage?.id ?? null,
      scores,
      myScore,
      (village, sx, sy) => {
        setSelectedTile(null);
        if (village.kind === "own") {
          setActiveVillage(village.id);
          setTargetVillage(null);
        } else {
          setTargetVillage(village);
          setTargetVillagePos({ x: sx, y: sy });
        }
      },
      (village) => setHoveredVillage(village),
    );
    scene.villages.setZoom(scene.root.scale.x);
  }, [
    allVillages,
    activeVillageId,
    targetVillage,
    scores,
    myScore,
    pixiReady,
    setActiveVillage,
    setHoveredVillage,
    setTargetVillage,
    setTargetVillagePos,
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

  useEffect(() => {
    if (!pixiReady) return;
    sceneRef.current?.setMovements(movements ?? [], villageCoords, ownIds);
  }, [movements, villageCoords, ownIds, pixiReady]);

  // Pan to the active village whenever it changes.
  useEffect(() => {
    if (!pixiReady) return;
    if (!activeVillageId) return;
    const village = villages[activeVillageId];
    if (!village) return;
    sceneRef.current?.centerOnVillage(village);
  }, [activeVillageId, activeVillageNonce, pixiReady, villages]);
}
