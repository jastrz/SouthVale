import { useEffect } from "react";
import { useAuthStore } from "../store/authStore";
import { useGameStateStore } from "../store/gameStateStore";
import { queryClient } from "../lib/query-client";

/**
 * Clears the React Query cache and game state whenever the auth token
 * becomes null. Renders nothing. This will be removed later.
 */
export function AuthCacheInvalidator() {
  const token = useAuthStore((s) => s.token);
  useEffect(() => {
    if (token === null) {
      queryClient.clear();
      useGameStateStore.getState().clear();
    }
  }, [token]);
  return null;
}
