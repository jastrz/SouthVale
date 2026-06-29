import { create } from "zustand";
import type {
  MapVillage,
  VillageListItemDto,
} from "../api/types";
import type { ViewMode } from "../types/view";

/**
 * Holds the live state of all villages the client currently knows about.
 */

export interface GameState {
  villages: Record<string, VillageListItemDto>;
  activeVillageId: string | null;
  hoveredVillage: MapVillage | null;
  currentView: ViewMode;

  targetVillage: MapVillage | null;
  selectedTile: { x: number; y: number } | null;

  // bulk hydration
  setVillages: (villages: VillageListItemDto[]) => void;
  upsertVillage: (village: VillageListItemDto) => void;
  removeVillage: (id: string) => void;
  clear: () => void;

  // selection
  setActiveVillage: (id: string | null) => void;
  setHoveredVillage: (village: MapVillage | null) => void;
  setTargetVillage: (village: MapVillage | null) => void;
  setSelectedTile: (tile: { x: number; y: number } | null) => void;
  setCurrentView: (view: ViewMode) => void;

}

export const useGameStateStore = create<GameState>((set) => ({
  villages: {},
  activeVillageId: null,
  hoveredVillage: null,
  currentView: "map",
  targetVillage: null,
  selectedTile: null,

  setVillages: (villages) =>
    set(() => ({
      villages: Object.fromEntries(villages.map((v) => [v.id, v])),
    })),

  upsertVillage: (village) =>
    set((state) => ({
      villages: { ...state.villages, [village.id]: village },
    })),

  removeVillage: (id) =>
    set((state) => {
      const next = { ...state.villages };
      delete next[id];
      return {
        villages: next,
        activeVillageId:
          state.activeVillageId === id ? null : state.activeVillageId,
      };
    }),

  setActiveVillage: (id) => set({ activeVillageId: id }),
  setHoveredVillage: (village) => set({ hoveredVillage: village }),
  setTargetVillage: (village) => set({ targetVillage: village }),
  setSelectedTile: (tile) => set({ selectedTile: tile }),
  setCurrentView: (view) => set({ currentView: view }),

  clear: () =>
    set({ villages: {}, activeVillageId: null, hoveredVillage: null, targetVillage: null, selectedTile: null }),
}));

// Convenience selectors — keep components from re-rendering on unrelated changes.
export const selectActiveVillage = (s: GameState): VillageListItemDto | undefined =>
  s.activeVillageId ? s.villages[s.activeVillageId] : undefined;

export const selectVillage =
  (id: string) =>
  (s: GameState): VillageListItemDto | undefined =>
    s.villages[id];
