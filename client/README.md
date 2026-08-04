# SouthVale Client

React frontend for the TownManager game. Real-time strategy UI: tile map
rendered with PixiJS, village management panels, reports, leaderboard.

## Stack

React 19, TypeScript, Vite · PixiJS 8 (map rendering) · TanStack Router +
Query · Zustand · Axios · SignalR · Tailwind CSS 4

## Dev

```bash
npm install
npm run dev     # needs the backend + Postgres running — see root README
```

## Structure

- `src/pixi/` — map renderer outside the React tree: `scene/` (MapScene +
  layers), `shaders/` (tree sway filter), `atlas.ts`, `tileData.ts`,
  `input/` (pan/zoom)
- `src/hooks/` — React↔Pixi bridge (`useMapRenderer`), app lifecycle
- `src/store/` — Zustand stores (game state, auth)
- `src/api/` — typed API client + TanStack Query hooks
- `src/components/` — UI: panels, popups, reports, trade, leaderboard

## Env

`VITE_API_URL`, `VITE_BASE_URL`, `VITE_CONTACT_EMAIL` — see `.env.example`
in the repo root.
