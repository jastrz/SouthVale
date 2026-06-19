import { create } from "zustand";

interface AuthState {
  token: string | null;
  email: string | null;
  username: string | null;
  setAuth: (token: string, email?: string, username?: string) => void;
  clearAuth: () => void;
}

export const useAuthStore = create<AuthState>()((set) => ({
  token: null,
  email: null,
  username: null,
  setAuth: (token, email, username) =>
    set({ token, email: email ?? null, username: username ?? null }),
  clearAuth: () => set({ token: null, email: null, username: null }),
}));
