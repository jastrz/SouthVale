import { useState, useEffect } from "react";
import { createRoot } from "react-dom/client";
import { QueryClientProvider } from "@tanstack/react-query";
import { RouterProvider } from "@tanstack/react-router";
import axios from "axios";
import "./index.css";
import { router } from "./routes/router";
import { queryClient } from "./lib/query-client";
import { Toaster } from "sonner";
import { AuthCacheInvalidator } from "./components/AuthCacheInvalidator";
import { useAuthStore } from "./store/authStore";
import { useSignalR } from "./hooks/useSignalR";
import { GlobalSpinner } from "./components/GlobalSpinner";

export function App() {
  useSignalR();
  const [ready, setReady] = useState(!!useAuthStore.getState().token);

  useEffect(() => {
    if (ready) return;

    axios
      .post(
        `${import.meta.env.VITE_API_URL}/auth/refresh`,
        {},
        { withCredentials: true },
      )
      .then(({ data }) => {
        useAuthStore.getState().setAuth(data.accessToken);
      })
      .catch(() => {
        /* no valid refresh token — stay logged out */
      })
      .finally(() => setReady(true));
  }, [ready]);

  if (!ready) return null;

  return (
    <>
      <AuthCacheInvalidator />
      <GlobalSpinner />
      <RouterProvider router={router} />
      <Toaster richColors theme="dark" position="bottom-center" closeButton />
    </>
  );
}

createRoot(document.getElementById("root")!).render(
  <QueryClientProvider client={queryClient}>
    <App />
  </QueryClientProvider>,
);
