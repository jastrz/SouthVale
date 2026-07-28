import { useMutation, useQuery } from "@tanstack/react-query";
import { toast } from "sonner";
import { api } from "../../lib/axios";
import { queryClient } from "../../lib/query-client";
import {
  type BuildRequest,
  type TrainRequest,
  type AttackRequest,
  type TransportRequest,
  type SettleRequest,
  type TradeRequest,
  type VillageListItemDto,
  type VillageDto,
  type PlayerVillageDto,
  type GetMapRequest,
  type MovementDto,
  type GameConfigDto,
  type ReportsResult,
  type VillageStatusDto,
  type LeaderboardResult,
} from "../types";

export const useGameConfig = () =>
  useQuery({
    queryKey: ["gameConfig"],
    queryFn: () => api.get<GameConfigDto>("/config").then((r) => r.data),
  });

export const useMyVillages = () =>
  useQuery({
    queryKey: ["villages"],
    queryFn: () =>
      api
        .get<VillageListItemDto[]>("/gameplay/me/villages")
        .then((r) => r.data),
  });

export const useMap = (request: GetMapRequest) =>
  useQuery({
    queryKey: ["map", request.cords.x, request.cords.y, request.radius],
    queryFn: () =>
      api
        .post<PlayerVillageDto[]>("/gameplay/map", request)
        .then((r) => r.data),
  });

export const useVillage = (id: string) =>
  useQuery({
    queryKey: ["village", id],
    queryFn: () =>
      api.get<VillageDto>(`/gameplay/village/${id}`).then((r) => r.data),
    enabled: !!id,
    refetchInterval: 10_000,
  });

export const usePlayerVillages = (username: string) =>
  useQuery({
    queryKey: ["villages", username],
    queryFn: () =>
      api
        .get<VillageDto[]>(`/gameplay/player/${username}/villages`)
        .then((r) => r.data),
    enabled: !!username,
  });

export const useMovements = () =>
  useQuery({
    queryKey: ["movements"],
    queryFn: () =>
      api.get<MovementDto[]>("/gameplay/me/movements").then((r) => r.data),
    // refetchInterval: 30_000,
  });

export const useVillageStatus = () =>
  useQuery({
    queryKey: ["villageStatus"],
    queryFn: () =>
      api
        .get<VillageStatusDto[]>("/gameplay/me/villages/status")
        .then((r) => r.data),
  });

const onError = (e: unknown) =>
  toast.error(e instanceof Error ? e.message : "Action failed");

export const useBuild = (villageId: string) =>
  useMutation({
    mutationFn: (data: BuildRequest) =>
      api.post(`/gameplay/village/${villageId}/build`, data),
    onSuccess: () => {
      toast.success("Upgrade queued");
      queryClient.invalidateQueries({ queryKey: ["village", villageId] });
      queryClient.invalidateQueries({ queryKey: ["villageStatus"] });
    },
    onError,
  });

export const useTrain = (villageId: string) =>
  useMutation({
    mutationFn: (data: TrainRequest) =>
      api.post(`/gameplay/village/${villageId}/train`, data),
    onSuccess: () => {
      toast.success("Training started");
      queryClient.invalidateQueries({ queryKey: ["village", villageId] });
      queryClient.invalidateQueries({ queryKey: ["villageStatus"] });
    },
    onError,
  });

export const useAttack = (villageId: string) =>
  useMutation({
    mutationFn: (data: AttackRequest) =>
      api.post(`/gameplay/village/${villageId}/attack`, data),
    onSuccess: () => {
      toast.success("Attack sent");
      queryClient.invalidateQueries({ queryKey: ["village", villageId] });
      queryClient.invalidateQueries({ queryKey: ["movements"] });
    },
    onError,
  });

export const useTransport = (villageId: string) =>
  useMutation({
    mutationFn: (data: TransportRequest) =>
      api.post(`/gameplay/village/${villageId}/transport`, data),
    onSuccess: () => {
      toast.success("Transport sent");
      queryClient.invalidateQueries({ queryKey: ["village", villageId] });
      queryClient.invalidateQueries({ queryKey: ["movements"] });
    },
    onError,
  });

export const useTrade = (villageId: string) =>
  useMutation({
    mutationFn: (data: TradeRequest) =>
      api.post(`/gameplay/village/${villageId}/trade`, data),
    onSuccess: () => {
      toast.success("Trade complete");
      queryClient.invalidateQueries({ queryKey: ["village", villageId] });
      queryClient.invalidateQueries({ queryKey: ["villageStatus"] });
    },
    onError,
  });

export const useSettle = (villageId: string) =>
  useMutation({
    mutationFn: async (data: SettleRequest) =>
      api.post(`/gameplay/village/${villageId}/settle`, data),
    onSuccess: () => {
      toast.success("Settler dispatched");
      queryClient.invalidateQueries({ queryKey: ["village", villageId] });
      queryClient.invalidateQueries({ queryKey: ["movements"] });
    },
    onError,
  });

export const useCancelBuild = (villageId: string) =>
  useMutation({
    mutationFn: (orderId: string) =>
      api.post(`/gameplay/build/${orderId}/cancel`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["village", villageId] });
      queryClient.invalidateQueries({ queryKey: ["villageStatus"] });
    },
  });

export const useCancelTrain = (villageId: string) =>
  useMutation({
    mutationFn: (orderId: string) =>
      api.post(`/gameplay/train/${orderId}/cancel`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["village", villageId] });
      queryClient.invalidateQueries({ queryKey: ["villageStatus"] });
    },
  });

export const useRenameVillage = (villageId: string) =>
  useMutation({
    mutationFn: (name: string) =>
      api.patch(`/gameplay/village/${villageId}/rename`, { name }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["village", villageId] });
      queryClient.invalidateQueries({ queryKey: ["villages"] });
    },
  });

export const useReports = (page = 1) =>
  useQuery({
    queryKey: ["reports", page],
    queryFn: () =>
      api
        .get<ReportsResult>("/gameplay/me/reports", {
          params: { page, pageSize: 10 },
        })
        .then((r) => r.data),
    placeholderData: (prev) => prev,
  });

export const useMarkReportsRead = () =>
  useMutation({
    mutationFn: () => api.post("/gameplay/me/reports/read"),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["reports"] }),
  });

export const useMarkReportRead = () =>
  useMutation({
    mutationFn: (reportId: string) =>
      api.post(`/gameplay/me/reports/${reportId}/read`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["reports"] }),
  });

export const useLeaderboard = (page = 1, pageSize = 10) =>
  useQuery({
    queryKey: ["leaderboard", page, pageSize],
    queryFn: () =>
      api
        .get<LeaderboardResult>("/gameplay/leaderboard", {
          params: { page, pageSize },
        })
        .then((r) => r.data),
    placeholderData: (prev) => prev,
  });
