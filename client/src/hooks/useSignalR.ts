import { useEffect, useRef } from "react";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { useAuthStore } from "../store/authStore";
import { queryClient } from "../lib/query-client";

export function useSignalR() {
  const token = useAuthStore((s) => s.token);
  const connectionRef = useRef<ReturnType<typeof createConnection> | null>(
    null,
  );

  useEffect(() => {
    if (!token) return;

    const connection = createConnection(token);
    connectionRef.current = connection;

    connection.on("VillageUpdated", (villageId: string) => {
      queryClient.invalidateQueries({ queryKey: ["village", villageId] });
      queryClient.invalidateQueries({ queryKey: ["villages"] });
    });

    connection.on("VillagesChanged", () => {
      queryClient.invalidateQueries({ queryKey: ["villages"] });
    });

    connection.on("MovementsChanged", () => {
      queryClient.invalidateQueries({ queryKey: ["movements"] });
    });

    connection.start().catch(() => {
      /* connection will be retried by SignalR's auto-reconnect */
    });

    return () => {
      connection.stop();
      connectionRef.current = null;
    };
  }, [token]);
}

function createConnection(token: string) {
  const url = `${import.meta.env.VITE_API_URL}/hubs/game`;
  return new HubConnectionBuilder()
    .withUrl(url, { accessTokenFactory: () => token })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build();
}
