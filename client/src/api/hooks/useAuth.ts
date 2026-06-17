import { useMutation } from "@tanstack/react-query";
import type { AuthResponse, LoginRequest, RegisterRequest } from "../types";
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
