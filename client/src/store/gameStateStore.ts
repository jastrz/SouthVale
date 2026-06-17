import { create } from "zustand";
import type {
  BuildOrderDto,
  ResourcesDto,
  TrainOrderDto,
  VillageDto,
} from "../api/types";

/**
 * Holds the live state of all villages the client currently knows about.
 */

export interface GameState {
  villages: Record<string, VillageDto>;
  activeVillageId: string | null;

  // bulk hydration
  setVillages: (villages: VillageDto[]) => void;
  upsertVillage: (village: VillageDto) => void;
  removeVillage: (id: string) => void;
  clear: () => void;

  // selection
  setActiveVillage: (id: string | null) => void;

  // fine-grained patches (SignalR-friendly)
  updateResources: (villageId: string, resources: ResourcesDto) => void;
  addBuildOrder: (villageId: string, order: BuildOrderDto) => void;
  removeBuildOrder: (villageId: string, orderId: string) => void;
  addTrainOrder: (villageId: string, order: TrainOrderDto) => void;
  removeTrainOrder: (villageId: string, orderId: string) => void;
}

export const useGameStateStore = create<GameState>((set) => ({
  villages: {},
  activeVillageId: null,

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

  updateResources: (villageId, resources) =>
    set((state) => {
      const village = state.villages[villageId];
      if (!village) return state;
      return {
        villages: {
          ...state.villages,
          [villageId]: { ...village, resources },
        },
      };
    }),

  addBuildOrder: (villageId, order) =>
    set((state) => {
      const village = state.villages[villageId];
      if (!village) return state;
      return {
        villages: {
          ...state.villages,
          [villageId]: {
            ...village,
            buildOrders: [...village.buildOrders, order],
          },
        },
      };
    }),

  removeBuildOrder: (villageId, orderId) =>
    set((state) => {
      const village = state.villages[villageId];
      if (!village) return state;
      return {
        villages: {
          ...state.villages,
          [villageId]: {
            ...village,
            buildOrders: village.buildOrders.filter((o) => o.id !== orderId),
          },
        },
      };
    }),

  addTrainOrder: (villageId, order) =>
    set((state) => {
      const village = state.villages[villageId];
      if (!village) return state;
      return {
        villages: {
          ...state.villages,
          [villageId]: {
            ...village,
            trainOrders: [...village.trainOrders, order],
          },
        },
      };
    }),

  removeTrainOrder: (villageId, orderId) =>
    set((state) => {
      const village = state.villages[villageId];
      if (!village) return state;
      return {
        villages: {
          ...state.villages,
          [villageId]: {
            ...village,
            trainOrders: village.trainOrders.filter((o) => o.id !== orderId),
          },
        },
      };
    }),

  clear: () => set({ villages: {}, activeVillageId: null }),
}));

// Convenience selectors — keep components from re-rendering on unrelated changes.
export const selectActiveVillage = (s: GameState): VillageDto | undefined =>
  s.activeVillageId ? s.villages[s.activeVillageId] : undefined;

export const selectVillage =
  (id: string) =>
  (s: GameState): VillageDto | undefined =>
    s.villages[id];
