import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";

export default defineConfig(({ mode }) => ({
  base: mode === "production" ? "/southvale/" : "/",
  plugins: [react(), tailwindcss()],
  server: {
    port: 3000,
    proxy: {
      "/auth": "http://localhost:5147",
      "/gameplay": "http://localhost:5147",
      "/config": "http://localhost:5147",
      "/admin/": "http://localhost:5147",
      "/health": "http://localhost:5147",
      "/hubs": "http://localhost:5147",
      "/scalar": "http://localhost:5147",
      "/openapi": "http://localhost:5147",
    },
  },
  optimizeDeps: {
    esbuildOptions: {
      target: "esnext",
    },
  },
  build: {
    target: "esnext",
  },
}));
