import { useMutation } from "@tanstack/react-query";
import type { AuthResponse, DeleteRequest, LoginRequest, RegisterRequest } from "../types";
import { api } from "../../lib/axios";

export const useLogin = () =>
  useMutation({
    mutationFn: (data: LoginRequest) =>
      api.post<AuthResponse>("/auth/login", data),
  });

export const useRegister = () =>
  useMutation({
    mutationFn: (data: RegisterRequest) =>
      api.post<AuthResponse>("/auth/register", data),
  });

export const useDelete = () =>
  useMutation({
    mutationFn: (data: DeleteRequest) =>
      api.post<boolean>("/auth/delete", data),
  })
