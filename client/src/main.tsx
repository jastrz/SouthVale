import { createRoot } from "react-dom/client";
import { QueryClientProvider } from "@tanstack/react-query";
import { RouterProvider } from "@tanstack/react-router";
import "./index.css";
import { router } from "./routes/router";
import { queryClient } from "./lib/query-client";
import { AuthCacheInvalidator } from "./components/AuthCacheInvalidator";

createRoot(document.getElementById("root")!).render(
  <QueryClientProvider client={queryClient}>
    <AuthCacheInvalidator />
    <RouterProvider router={router} />
  </QueryClientProvider>,
);
