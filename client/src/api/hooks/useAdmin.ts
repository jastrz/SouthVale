import { useMutation, useQuery } from "@tanstack/react-query";
import { toast } from "sonner";
import { api } from "../../lib/axios";
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
