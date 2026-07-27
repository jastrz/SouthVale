## TownManager (SouthVale)

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Website](https://img.shields.io/website?url=https%3A%2F%2Fkubastuff.eu%2Fsouthvale)](https://kubastuff.eu/southvale)
[![API Docs](https://img.shields.io/badge/API_Docs-Scalar-8A2BE2)](https://kubastuff.eu/southvale/api/scalar)
[![Architecture](https://img.shields.io/badge/Architecture-lightgrey)](ARCHITECTURE.md)

Real-time multiplayer browser strategy game. Players build and extend their villages, train beer-fueled armies and attack opponents.

**Play:** [kubastuff.eu/southvale](https://kubastuff.eu/southvale) 

**API:** [kubastuff.eu/southvale/api/scalar](https://kubastuff.eu/southvale/api/scalar)


## Quick Start

```bash
# Full stack
docker compose up -d

# Or dev mode
cd backend/TownManager.Api && dotnet run
cd client && npm install && npm run dev
```

## Features

- 4-resource economy (wood, clay, iron, beer) 
- 11 building types (townhall, barracks, stables, warehouse, cranny, wall, trade post, production buildings)
- 6 troop types (swordsmen, archers, settlers, dogs, llama riders, horsemen)
- Attack, transport, and settle mechanics 
- LLM-driven players/bots
- Barbarian NPC villages that grow and attack periodically
- PixiJS map with pan, zoom, and village selection
- Map editor for terrain authoring

## Tech Stack

**Backend:** .NET 10, Clean Architecture (Domain/Application/Infrastructure/Api), CQRS with MediatR, PostgreSQL + EF Core, Hangfire (background jobs), SignalR (real-time), JWT + ASP.NET Identity, FluentValidation, Serilog, Scalar (OpenAPI)

**Frontend:** React 19, TypeScript, PixiJS 8 (map rendering), TanStack Router + Query, Zustand (client state), Axios, SignalR, Tailwind CSS, Vite

**Infrastructure:** Docker Compose (PostgreSQL, pgAdmin, backend)

## Links

- [ARCHITECTURE.md](ARCHITECTURE.md)
- [LICENSE](LICENSE)

