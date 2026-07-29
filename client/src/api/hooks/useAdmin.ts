import { useMutation, useQuery } from "@tanstack/react-query";
import { toast } from "sonner";
import { api } from "../../lib/axios";
import { queryClient } from "../../lib/query-client";
import type { AddResourcesRequest } from "../types";

export interface VillageItem { id: string; name: string; coordinates: { x: number; y: number } }

export const useAdminVillages = () =>
  useQuery({
    queryKey: ["admin", "villages"],
    queryFn: () => api.get<VillageItem[]>("/admin/villages").then((r) => r.data),
  });

export const useAddResources = () =>
  useMutation({
    mutationFn: ({
      villageId,
      resources,
    }: {
      villageId: string;
      resources: AddResourcesRequest;
    }) => api.post(`/admin/village/${villageId}/resources`, resources),
    onSuccess: () => toast.success("Resources added"),
    onError: (e) => toast.error(String(e)),
  });

export const useFullResources = () =>
  useMutation({
    mutationFn: () => api.post("/admin/villages/resources/full"),
    onSuccess: (r) => toast.success(`Filled ${r.data.filled} villages`),
    onError: (e) => toast.error(String(e)),
  });

export const useTickBarbarian = () =>
  useMutation({
    mutationFn: () => api.post("/admin/tick/barbarian"),
    onSuccess: () => toast.success("Barbarian tick triggered"),
    onError: (e) => toast.error(String(e)),
  });

export const useTickLlm = () =>
  useMutation({
    mutationFn: () => api.post("/admin/tick/llm"),
    onSuccess: () => toast.success("LLM tick triggered"),
    onError: (e) => toast.error(String(e)),
  });

export interface GameConfig { travelSpeedMultiplier: number; resourcesProductionMultiplier: number; buildSpeedMultiplier: number; trainSpeedMultiplier: number; maxBarbarianVillages: number }

export const useGameConfig = () =>
  useQuery({
    queryKey: ["admin", "config"],
    queryFn: () => api.get<GameConfig>("/admin/config").then((r) => r.data),
  });

export const useUpdateGameConfig = () =>
  useMutation({
    mutationFn: (config: Partial<GameConfig>) => api.put("/admin/config", config),
    onSuccess: () => { toast.success("Config updated"); queryClient.invalidateQueries({ queryKey: ["admin", "config"] }); queryClient.invalidateQueries({ queryKey: ["game-config"] }); },
    onError: (e) => toast.error(String(e)),
  });

export const useResetDb = () =>
  useMutation({
    mutationFn: (password: string) => api.post("/admin/reset", { password }),
    onSuccess: () => { toast.success("Database reset"); queryClient.clear(); },
    onError: (e) => toast.error(String(e)),
  });
