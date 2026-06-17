import { useMutation, useQuery } from "@tanstack/react-query";
import { api } from "../../lib/axios";
import { queryClient } from "../../lib/query-client";
import type {
  BuildRequest,
  TrainRequest,
  AttackRequest,
  SettleRequest,
} from "../types";

export const useMyVillages = () =>
  useQuery({
    queryKey: ["villages"],
    queryFn: () => api.get("/gameplay/me/villages").then((r) => r.data),
  });

export const useVillage = (id: string) =>
  useQuery({
    queryKey: ["village", id],
    queryFn: () => api.get(`/gameplay/village/${id}`).then((r) => r.data),
    enabled: !!id,
  });

export const usePlayerVillages = (username: string) =>
  useQuery({
    queryKey: ["villages", username],
    queryFn: () =>
      api.get(`/gameplay/player/${username}/villages`).then((r) => r.data),
    enabled: !!username,
  });

export const useBuild = (villageId: string) =>
  useMutation({
    mutationFn: (data: BuildRequest) =>
      api.post(`/gameplay/village/${villageId}/build`, data),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: ["village", villageId] }),
  });

export const useTrain = (villageId: string) =>
  useMutation({
    mutationFn: (data: TrainRequest) =>
      api.post(`/gameplay/village/${villageId}/train`, data),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: ["village", villageId] }),
  });

export const useAttack = (villageId: string) =>
  useMutation({
    mutationFn: (data: AttackRequest) =>
      api.post(`/gameplay/village/${villageId}/attack`, data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["movements"] }),
  });

export const useSettle = (villageId: string) =>
  useMutation({
    mutationFn: (data: SettleRequest) =>
      api.post(`/gameplay/village/${villageId}/settle`, data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["villages"] }),
  });
