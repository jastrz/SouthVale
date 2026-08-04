# Game Docs

Internal reference for game rules, backend behavior, and operations. Code
and data files use the names shown here. API endpoints are documented in
Scalar (served by the backend); this file covers mechanics and internals.

<a name="scope-and-conventions"></a>
## Scope and conventions

- All rates — production, upkeep, training, travel — are **hourly rates**
  applied to real elapsed time. Nothing is simulated in the background.
- A **tick** materializes elapsed time: production added, upkeep paid.
  Every command ticks the village it touches before spending resources;
  read-only queries use a projection instead.
- **Effects** are building effects aggregated by
  `BuildingConfig.AggregateEffects`. Most apply within one village;
  attack and training multipliers use the best applicable building level
  across the player's villages.
- **Stored resources** are the values persisted in the database. They can
  be stale between ticks — current reads must use the projection, never
  raw stored values.
- **Domain events** (e.g. `TroopsStarvedEvent`) are collected on entities
  and published by `AppDbContext` via MediatR *after* the save commits —
  event handlers run only when the change is durable (transaction-safe
  pipeline).

## Contents

- [Scope and conventions](#scope-and-conventions)
- [Map](#map)
- [Buildings](#buildings)
  - [Build scheduling](#build-scheduling)
- [Troops & Training](#troops-training)
- [Resource Management](#resource-management)
  - [Production & Tick](#resources-production)
  - [Upkeep & Starvation](#resources-upkeep)
  - [Storage](#resources-storage)
  - [Trade](#resources-trade)
  - [Spending & Transfers](#resources-spending)
- [Troop Movements](#troop-movements)
  - [Lifecycle](#movement-lifecycle)
  - [Travel time](#movement-travel-time)
  - [Attack](#movement-attack)
  - [Transport](#movement-transport)
  - [Settle](#movement-settle)
  - [Return](#movement-return)
- [Reports](#reports)
- [Leaderboard & Score](#leaderboard-score)
- [BOTS](#bots)
  - [LLM Player](#llm-player)
  - [Barbarians](#barbarians)
  - [Tick Intervals (LLM / Barbarian players)](#tick-intervals)
- [Config Sources](#config-sources)
- [Real-Time Notifications](#real-time-notifications)
- [Guests](#accounts-auth)
- [Operations](#operations)
- [Player Display (map village labels)](#player-display)
  - [PlayerName color](#playername-color)
  - [Selection arrows](#selection-arrows)

<a name="map"></a>
# 1. Map

The world is a square grid (size from `GameSettings.MapSize`). Terrain
comes from `data/terrain.json`: water tiles (`W`) and decoration spots are
blocked — no village can be built or settled there.

**Villages on the map:**

- New players and bots spawn on free tiles, with starting resources
  (`GameSettings.StartingResources`).
- Barbarian villages are replenished over time to keep population up.
- Destroying a barbarian village frees its tile for new settlements.

Free tiles are picked randomly from all empty walkable tiles
(`MapService.GetFreeTilesAsync`) — used for registration, seeding and
barbarian replenish. Bots perceive the world through the same radius query
as players (see Scalar for `/gameplay/map`).

<a name="buildings"></a>
# 2. Buildings

Levels, costs, times, and effects come from `data/buildings.csv`
(`BuildingConfig`).
- Effects aggregate across all buildings in the village.
- Attack and training multipliers use the best applicable level across the
  player's villages.

| Building    | Description |
| ----------- | ----------- |
| TownHall    | Build speed multiplier (faster upgrades). |
| Warehouse   | Raises storage cap for all four resources. |
| WoodCutter  | Produces wood per hour. |
| ClayPit     | Produces clay per hour. |
| IronMine    | Produces iron per hour. |
| Brewery     | Produces beer per hour (pays troop upkeep). |
| Barracks    | Trains infantry (Swordsman, Archer, Dogs); training speed + infantry attack multiplier. |
| Stable      | Trains cavalry (Horsemen, LlamaRiders); training speed + cavalry attack multiplier. |
| Wall        | Defense multiplier (protects village in combat). |
| Cranny      | Hides resources from loot up to its capacity. |
| TradePost   | Enables trading; TradeRate determines conversion. |
| Granary     | Obsolete (marked `[Obsolete]`, no active effect). |

<a name="build-scheduling"></a>
## Build scheduling

- `CreateBuildOrderCommand` sets target level to max(current level, last queued
  target) + 1.
- Cost comes from `BuildingConfig` for the target level.
- Cost is deducted immediately after the village tick.
- Duration = `UpgradeTime` / `BuildSpeedMultiplier` (TownHall) /
  `GameSettings.BuildSpeedMultiplier`.
- Each order starts after the last queued order's `CompletesAt`.
- A Hangfire job (`BuildOrderResolutionJob`) fires at `CompletesAt`.
- Resolution sets the building to `TargetLevel`, creating it if missing.
- Resolution removes the order and sends a notification.
- If a job was rescheduled, the old job is skipped. Its ID must match the
  order's stored `JobId`.

<a name="troops-training"></a>
# 3. Troops & Training

Stats (attack, defense, carry capacity, speed) and costs loaded from
`data/troops.csv` (`TroopsConfig`).

| Troop        | Trained at | Role |
| ------------ | ---------- | ---- |
| Swordsman    | Barracks   | Infantry, high attack (offense). |
| Archer       | Barracks   | Infantry, high defense. |
| Dogs         | Barracks   | Low cost, high speed, attack. |
| Horsemen     | Stable     | Cavalry, high attack, fast. |
| LlamaRiders  | Stable     | Balanced, high carry capacity (resources). |
| Settler      | Barracks   | Found new villages; no combat stats. |

## Training

- `CreateTrainOrderCommand` deducts resources immediately.
- A training building is required: Barracks or Stable at level ≥ 1.
- Cost = base cost × count.
- New orders append after the last queued order's completion time.
- Training time per unit is scaled by `BarracksTrainingSpeed` or
  `StableTrainingSpeed` (building level), then by
  `GameSettings.TrainSpeedMultiplier`.
- A Hangfire job per unit adds the unit to the garrison and reschedules until
  the order is done.
- Settler cost grows geometrically: each additional settler costs double the
  previous one.
- Settler base cost also scales with the number of villages already owned.
- Upkeep: every troop consumes beer per hour (`Upkeep`, `UpkeepMultiplier`),
  see [Resource Management](#resources-upkeep). No beer → starvation.
- Combat: attack power from attack stat × building attack multipliers,
  defense from defense stat × wall multiplier; travel speed = slowest troop
  in the group, see [Troop Movements](#movement-travel-time).

<a name="resource-management"></a>
# 4. Resource Management

Four resources: **Wood, Clay, Iron, Beer**. Produced per hour by resource
buildings (WoodCutter, ClayPit, IronMine, Brewery), multiplied by
`GameSettings.ResourcesProductionMultiplier`.

<a name="resources-production"></a>
## Production & Tick

Lazy tick model: production is computed from elapsed time since
`LastTickAt`, not accumulated in the background.

- `Village.Tick(effects)` mutates the village.
- It produces resources, pays upkeep, resolves starvation, caps resources to
  warehouse capacity, and updates `LastTickAt`.
- `VillageTickService` calls it periodically for all villages.
- Commands and queries touching a village can also call it lazily.
- `GetCurrentResources(effects)` — read-only projection of the same math
  (production − upkeep, capped), used for display without mutation.
- Every command (build, train, attack, transport, trade) ticks the village
  first, so stored resources are always current at spend time.

**Persistence**:

- Stored resource values are materialized (`Tick` + `SaveChanges`) when a
  village-modifying action runs or the village tick job fires.
- Between those actions, the database can hold stale numbers.
- Reads must use `GetCurrentResources`, never raw stored values.

<a name="resources-upkeep"></a>
## Upkeep & Starvation

Troops consume **Beer** upkeep per hour (`GameSettings.UpkeepMultiplier`).
If upkeep drives beer negative:

- `ResolveStarvation` removes troops in cheapest-upkeep order until the
  deficit is covered.
- Starved troops emit `TroopsStarvedEvent` as a notification.

<a name="resources-storage"></a>
## Storage

Each resource capped at `WarehouseCapacity` (from Warehouse level,
aggregated via `BuildingConfig.AggregateEffects`). Cap applied on every
tick — production above cap is lost. Build Warehouse to raise it.

<a name="resources-trade"></a>
## Trade

`TradeResourcesCommand` (TradePost required, level ≥ 1):

- Give one resource type and receive another.
- `receive = floor(give × TradeRate)`.
- `TradeRate` comes from TradePost level. A rate that produces too little is
  rejected.
- Trading a resource for itself is not allowed.
- The received amount must fit in warehouse capacity.

<a name="resources-spending"></a>
## Spending & Transfers

- Build/train orders deduct resources immediately at creation.
- Transport moves troops + resources between villages
  (`CarriedResources`).
- Attack loot: cranny-protected resources are safe; the rest is carried
  home by surviving attackers (bounded by carry capacity), added to home
  storage on return.

<a name="troop-movements"></a>
# 5. Troop Movements

<a name="movement-lifecycle"></a>
## Lifecycle

- An attack, transport, or settle order is created.
- Troops are removed from the garrison and resources are deducted.
- The movement is saved with departure and arrival times.
- A Hangfire job fires when the movement arrives.
- The resolver for that movement type runs in a DB transaction.
- The movement is marked done with its completion time.
- Both source and target players receive `MovementsChanged`.

Movement types: **Attack**, **Transport**, **Settle**, **Return**.

- Return is never ordered by the player — created automatically by
  attack/settle resolution.
- Every resolver emits a report (`ReportFactory`).

<a name="movement-travel-time"></a>
## Travel time

`TravelTimeCalculator` uses Manhattan distance / troop speed /
`GameSettings.TravelSpeedMultiplier`.
- Group speed is the slowest troop type in the group.
- This applies to the outbound attack and the return trip with survivors.

<a name="movement-attack"></a>
## Attack

`AttackMovementResolver`:

1. Target ticked (resource production before combat).
2. Attack bonuses empire-wide: best Barracks/Stable level across all
   attacker's villages.
3. Cranny protects resources up to its capacity. Only resources above that cap
   can be looted.
4. `CombatResolver` calculates attack power as Σ troops × attack × building
   multiplier.
5. It calculates defense power as Σ troops × defense × wall multiplier.
6. Losses scale with the power ratio; the outmatched side loses most of its
   troops.
7. Loot is taken from the target up to the surviving attackers' carry capacity.
8. Attack and defense reports are created. Defense reports are created only for
   human targets.
9. Survivors return home as a **Return** movement carrying loot.
10. A barbarian village is deleted from the map when attackers wipe its
    defenders.

<a name="movement-transport"></a>
## Transport

`TransportMovementResolver`: adds troops + carried resources to target
village, transport report to target's owner. No combat.

<a name="movement-settle"></a>
## Settle

`SettleMovementResolver`: tile free → create new starter village for the
player. Tile occupied → settlers return home (Return), "settle failed"
report, no new village. Village count capped at
`GameSettings.MaxVillagesPerPlayer` (checked at order creation).

<a name="movement-return"></a>
## Return

`ReturnMovementResolver`: troops (and loot/resources) added back to home
village, return report. Handles both attack survivors-with-loot and failed
settlers.

<a name="reports"></a>
# 6. Reports

Resolvers create outcome records for player actions via `ReportFactory`.
Report text includes troops sent, lost, and survived; loot; and coordinates.
Reports are fetched at `/gameplay/me/reports`. Per-report and bulk read state
use `/read`. Types:

| Report | When |
| ------ | ---- |
| Attack | attacker sees attack result |
| Defense | defender sees incoming attack result (human targets only) |
| Return | troops/loot back home |
| Transport | target owner sees delivered troops/resources |
| Settle / SettleFailed | new village / occupied tile |
| AttackCancelled | target village vanished, troops sent home |
| Starvation | troops starved from beer deficit |

<a name="leaderboard-score"></a>
# 7. Leaderboard & Score

The leaderboard ranks every player by a single score, computed on the
fly each time it is opened — nothing is stored.

A player's score is the sum over all of their villages of:

- **Troop score** — every troop in the garrison or currently on the move,
  weighted per type by `TroopConfig.Score` (from `data/troops.csv`).
- **Building score** — every building, weighted by its level's
  `BuildingLevelConfig.Score` (from `data/buildings.csv`).

Served at `/gameplay/leaderboard` (paginated; each entry carries its
rank). The score also drives the player name colors on the map (see
[PlayerName color](#playername-color)).

<a name="bots"></a>
# 8. Bots

<a name="llm-player"></a>
## LLM Player

Bot players controlled by an LLM (OpenAI-compatible API). Seeded on startup
via `LlmPlayerSeeder` — several bots, each with a personality: Aggressive /
Defensive / Economic. Gated by `Features:UseLlmPlayers`, and
`LlmPlayer.ApiKey` must be set.

Per tick, for every bot:

1. Load bot's villages (+ build/train queues), in-flight movements, incoming
   attacks, and nearby villages within a radius.
2. Build prompt: personality description, game config (building/troop costs),
   per-village state (resources, troops, buildings, queues), nearby villages,
   available action schema.
3. Call `/chat/completions` (thinking enabled by default) with retries;
   strip markdown fences, parse JSON array of actions.
4. Validate each action through the same commands as human players (build,
   train, attack, transport, settle) — resource checks apply, invalid actions
   are counted as failures.

Action types: `build`, `train`, `attack`, `transport`, `settle`. Max
`LlmPlayer.MaxActionsPerTick` actions per tick. Activity logged via
`LlmActivity` (console + file). Admin can trigger a tick manually.

<a name="barbarians"></a>
## Barbarians

Deterministic AI villages — no LLM, simple heuristic service. One shared
"Barbarian" player (fixed ID `0000...0001`), villages of type
`VillageType.Barbarian`, yellow labels on map. Gated by
`Features:UseBarbarians`.

Per tick (`BarbarianTickService`), per barb village, in random order:

- **AutoBuild**: queue upgrades for starting buildings until level
  `BarbarianConfig.MaxBuildingLevel`.
- **AutoTrain**: keep troops under per-village cap = best player's village
  total troops × `MaxTroopRatio`, and under per-type caps; trains only
  what's affordable.
- **TryAttack**: if troops exist, attack cooldown elapsed, no attack
  in flight — pick random nearby village within range with *more* troops
  than self, send half of each troop type.

**Replenish**: after all villages tick, spawn new barb villages on free map
tiles until `TargetPopulation`. Barb villages are destroyed when an
attacker wipes their defenders (removed from map, conquerable space).

<a name="tick-intervals"></a>
## Tick Intervals (LLM / Barbarian players)

- Raw cron only: `LlmPlayer__TickIntervalCron`, `Barbarian__TickIntervalCron`.
- Admin accepts any cron expression, validated via NCrontab (400 on invalid).
- Admin changes apply live but reset to env on restart.
- Example: `0 6-23 * * *` → hourly 06:00–23:00.

<a name="config-sources"></a>
# 9. Config Sources

Three layers, in precedence order (later overrides earlier):

1. **Static data files** (`backend/data/`, loaded at startup, static for
   process lifetime):
   - `buildings.csv` → `BuildingConfig` (levels, costs, times, effects,
     score)
   - `troops.csv` → `TroopsConfig` (stats, costs, training time, upkeep)
   - `terrain.json` → `MapTerrain` (grid + blocked tiles)
2. **Environment / .env** (bound in `Program.cs`):
   - `GameSettings__*` multipliers: Travel/ResourcesProduction/Upkeep/
     BuildSpeed/TrainSpeed
   - `Barbarian__TargetPopulation`, `Barbarian__MaxTroopRatio`
   - `Features__*` feature flags, `LlmPlayer__*` / `Barbarian__*` cron
     options
   - Applies at startup only.
3. **Admin runtime** (`GET`/`PUT /admin/config`, admin-authorized):
   - Edit multipliers (each change bumps `ConfigVersion`), barbarian
     target population
   - Cron expressions validated via NCrontab (400 on invalid), re-register
     Hangfire jobs live
   - Applies immediately, resets to env on restart
   - Other admin controls: manual LLM tick trigger, soft game reset
     (keeps registered users, re-seeds)

<a name="real-time-notifications"></a>
# 10. Real-Time Notifications

SignalR hub at `/hubs/game` (authenticated; per-user delivery). Events
sent by `IGameNotificationService` (commands + resolution jobs):

| Event | Sent when |
| ----- | --------- |
| VillageUpdated | village state changed (build/train resolved, attack/transport/settle resolved, trade) |
| VillagesChanged | village created (settle success, registration) |
| MovementsChanged | movement created or resolved (both source and target player) |
| ReportCreated | any report generated |
| VillageDestroyed | barbarian village wiped |

Clients should refetch the relevant resource on each event (payload is
just the trigger).

<a name="accounts-auth"></a>
# 11. Guests

Play without an account: `register-guest` creates an anonymous player
(with starter village on a free tile). `claim-guest` converts it into a
real email/password account later.

- Non-guest users: JWT access + refresh tokens via
  register/login/refresh/logout.
- Account deletion removes game data.
- Admin role seeded only when `Admin:Password` is set.
- Destructive admin ops additionally require password confirmation.

<a name="operations"></a>
# 12. Operations

**Hangfire jobs**:

- Recurring:
  - `village-tick` — materializes production/upkeep for all villages
  - `barbarian-tick` — barbarian NPCs act
  - `llm-player-tick` — LLM bots act
  - Crons come from config/env, editable live by admin.
- One-shot: build/train order resolution (fired per order/unit at
  completion) and movement resolution (fired at arrival) — see
  [Build scheduling](#build-scheduling), [Training](#troops-training),
  [Troop Movements](#movement-lifecycle).

**Admin controls** (`/admin/*`, admin role): village list, inject
resources into a village, fill all villages' resources, manual barbarian /
LLM tick triggers, soft reset, config view/edit (see
[Config Sources](#config-sources)).

**Soft reset** (`/admin/reset`, password-confirmed): keeps registered
(non-guest) users and admins, deletes all game data + guest accounts,
drops the Hangfire schema, recreates starter villages, restarts the app
for re-seeding.

**Backups**: a dedicated sidecar container
(`prodrigestivill/postgres-backup-local`) dumps the PostgreSQL database
into the `./backups` directory on a schedule.

- Schedule: `BACKUP_SCHEDULE` (default: daily)
- Retention: `BACKUP_KEEP_DAYS` / `BACKUP_KEEP_WEEKS` / `BACKUP_KEEP_MONTHS`
  — how many dumps of each tier to keep

**Startup seeding** (in order): barbarian player/villages (when
`Features:UseBarbarians`), LLM bot players (when `Features:UseLlmPlayers`),
admin user (when `Admin:Password` set), test data (dev only).

<a name="player-display"></a>
# 13. Player Display (map village labels)

Labels above village markers, top to bottom: **PlayerName** (large bold,
colored), village name (medium white), population (small white).

<a name="playername-color"></a>
## PlayerName color

`ratio = playerScore / myScore` (leaderboard score, from
`/gameplay/leaderboard`):

| ratio     | color      |
| --------- | ---------- |
| >= 1.5    | red        |
| 0.5 – 1.5 | white      |
| < 0.5     | green      |
| barbarian | yellow (overrides) |

<a name="selection-arrows"></a>
## Selection arrows

Own active village = green arrow, enemy target = red arrow, bobbing on Y
above the village. Created only for selected villages, counter-scaled with
zoom.
