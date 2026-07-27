# Architecture

## Layers (Clean Architecture)

```
Api → Application ← Infrastructure
         ↓
       Domain
```

- **Domain** — entities, enums, game config, domain events. Zero dependencies.
- **Application** — CQRS commands/queries, handlers, DTOs, service interfaces.
- **Infrastructure** — EF Core, Hangfire jobs, LLM HTTP client, Identity. 
- **Api** — ASP.NET host, minimal API endpoints, SignalR hubs, middleware. Wires DI.

## Client

React 19 with TanStack Router (auth-guarded routes) + TanStack Query (server cache invalidated by SignalR pushes) + Zustand (UI state). PixiJS 8 renders tile map outside React tree - `usePixiApp` hook bridges them.

## Request Flow

```
HTTP → Endpoint → MediatR → Handler → Domain logic → DbContext → DB
                                     ↘ SignalR hub → connected clients
```

`TransactionBehavior` MediatR pipeline wraps commands in EF Core transactions.

## Background Jobs

Triggered using Hangfire.

**Scheduled one-shot**:
- `BuildOrderResolutionJob` — completes building orders, applies effects
- `TrainOrderResolutionJob` — completes troop training
- `TroopMovementResolutionJob` — resolves movement arrival (attack, return, settle, transport), delegates to type-specific resolver

**Recurring**:
- `BarbarianTickJob` — barbarian NPCs decide and queue actions
- `LlmPlayerJob` — LLM-bot players decide and queue actions

Any handler can schedule future work via `IJobScheduler` interface.

## Real-Time (SignalR)

`GameHub` pushes game state updates. `ConnectedUserTracker` maps userId → connectionId.

## Tests

3 test projects — Domain, Application, Integration. Partial coverage.
