# TownManager

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Real-time multiplayer browser-based strategy game (Travian/Tribal Wars style). Players build villages, train armies, attack opponents, and climb the leaderboard — all rendered on a PixiJS-powered world map.

## Tech Stack

**Backend:** .NET 10, Clean Architecture (Domain/Application/Infrastructure/Api), CQRS with MediatR, PostgreSQL + EF Core, Hangfire (background jobs), SignalR (real-time), JWT + ASP.NET Identity, FluentValidation, Serilog, Scalar (OpenAPI)

**Frontend:** React 19, TypeScript, PixiJS 8 (map rendering), TanStack Router + Query, Zustand (client state), Axios, SignalR, Tailwind CSS, Vite

**Infrastructure:** Docker Compose (PostgreSQL, pgAdmin, backend)

## Architecture

```
Api → Application ← Infrastructure
         ↕                ↕
        Domain     (EF Core, Hangfire, Identity)
```

Domain has zero external dependencies. Application references only interfaces. Infrastructure implements persistence, identity, job scheduling.

## Features

- 4-resource economy (wood, clay, iron, crop) with production ticking
- 5 building types (headquarters, barracks, warehouse, production buildings)
- 3 troop types (swordsmen, archers, settlers) with configurable training
- Attack, transport, and settle mechanics with travel time
- Combat resolution system
- Barbarian NPC villages that grow and attack periodically
- SignalR push for real-time UI updates
- Hangfire-scheduled build/train/movement resolution (survives restarts)
- PixiJS map with pan, zoom, and village selection
- Interactive API docs at `/scalar` via Scalar + OpenAPI
- Auto-tiling terrain with props (trees, bushes)
- Map editor for terrain authoring

## Getting Started

```bash
# Backend
cd backend/TownManager.Api
dotnet run

# Client
cd client
npm install
npm run dev

# Full stack
docker compose up
```

## Project Structure

```
backend/
├── TownManager.Domain/        # Entities, enums, configs, domain services
├── TownManager.Application/   # CQRS commands/queries, DTOs, interfaces
├── TownManager.Infrastructure/ # EF Core, Hangfire jobs, auth, repos
├── TownManager.Api/           # Minimal API endpoints, SignalR hub
├── TownManager.Domain.Tests/
├── TownManager.Application.Tests/
└── TownManager.IntegrationTests/  # Testcontainers-based integration tests

client/src/
├── api/          # TanStack Query hooks, types
├── components/   # React UI (village, transport, overview panels)
├── hooks/        # SignalR, Pixi bridge, game loop
├── pixi/         # PixiJS scene, layers, entities, input, animation
├── pages/        # Game, login, register, map editor
├── routes/       # TanStack Router setup
├── store/        # Zustand stores (auth, game state)
└── styles/       # Tailwind config
```

## License

MIT — see [LICENSE](LICENSE). Copyright © 2026 jastrz.
