import { create } from "zustand";

function load<T>(key: string): T | null {
  try {
    const raw = localStorage.getItem(key);
    return raw ? (JSON.parse(raw) as T) : null;
  } catch {
    return null;
  }
}

function save(key: string, value: unknown) {
  try {
    localStorage.setItem(key, JSON.stringify(value));
  } catch { /* quota exceeded, ignore */ }
}

function remove(key: string) {
  try {
    localStorage.removeItem(key);
  } catch { /* ignore */ }
}

interface AuthState {
  token: string | null;
  email: string | null;
  username: string | null;
  setAuth: (token: string, email?: string, username?: string) => void;
  clearAuth: () => void;
}

export const useAuthStore = create<AuthState>()((set) => ({
  token: load<string>("token"),
  email: load<string>("email"),
  username: load<string>("username"),
  setAuth: (token, email, username) =>
    set((state) => {
      save("token", token);
      const nextEmail = email ?? state.email;
      const nextUsername = username ?? state.username;
      if (nextEmail) save("email", nextEmail);
      if (nextUsername) save("username", nextUsername);
      return { token, email: nextEmail, username: nextUsername };
    }),
  clearAuth: () => {
    remove("token");
    remove("email");
    remove("username");
    set({ token: null, email: null, username: null });
  },
}));
