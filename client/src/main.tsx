import { useState, useEffect } from "react";
import { createRoot } from "react-dom/client";
import { QueryClientProvider } from "@tanstack/react-query";
import { RouterProvider } from "@tanstack/react-router";
import axios from "axios";
import "./index.css";
import { router } from "./routes/router";
import { queryClient } from "./lib/query-client";
import { AuthCacheInvalidator } from "./components/AuthCacheInvalidator";
import { useAuthStore } from "./store/authStore";

function App() {
  const [ready, setReady] = useState(false);

  useEffect(() => {
    if (useAuthStore.getState().token) {
      setReady(true);
      return;
    }

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
  }, []);

  if (!ready) return null;

  return (
    <>
      <AuthCacheInvalidator />
      <RouterProvider router={router} />
    </>
  );
}

createRoot(document.getElementById("root")!).render(
  <QueryClientProvider client={queryClient}>
    <App />
  </QueryClientProvider>,
);
