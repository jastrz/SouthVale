export type BuildingType =
  | "Warehouse"
  | "Barracks"
  | "IronMine"
  | "WoodCutter"
  | "Brewery"
  | "ClayPit"
  | "Stable"
  | "Wall"
  | "Cranny"
  | "TradePost"
  | "TownHall";

export type TroopType = "Swordsman" | "Archer" | "Settler" | "Dogs" | "Horsemen" | "LlamaRiders";

export interface TroopEntry {
  troopType: TroopType;
  count: number;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  username: string;
}

export interface DeleteRequest {
  password: string;
}

export interface AuthResponse {
  accessToken: string;
  username?: string;
}

export interface BuildRequest {
  buildingType: BuildingType;
}

export interface Coordinates {
  x: number;
  y: number;
}

export interface SettleRequest {
  target: Coordinates;
}

export interface TrainRequest {
  orders: TroopEntry[];
}

export interface AttackRequest {
  targetVillageId: string;
  troops: TroopEntry[];
}

export interface TransportRequest {
  targetVillageId: string;
  troops: TroopEntry[];
  resources: ResourcesDto;
}

export interface ResourcesDto {
  wood: number;
  clay: number;
  iron: number;
  beer: number;
}

export interface BuildingLevelConfigDto {
  level: number;
  upgradeCost: ResourcesDto;
  upgradeTime: string;
  warehouseCapacity: number;
  granaryCapacity: number;
  productionPerHour: ResourcesDto | null;
  trainingSpeedMultiplier: number;
  defenseMultiplier: number;
  crannyCapacity: number;
  tradeRate: number;
  buildSpeedMultiplier: number;
}

export interface TroopConfigDto {
  type: string;
  trainingCost: ResourcesDto;
  trainingTime: string;
  attack: number;
  defense: number;
  carryCapacity: number;
  speed: number;
  trainedAt: string;
}

export interface GameConfigDto {
  buildings: Record<string, BuildingLevelConfigDto[]>;
  troops: Record<string, TroopConfigDto>;
  maxVillagesPerPlayer: number;
}

export interface TroopsDto {
  swordsmen: number;
  archers: number;
  settlers: number;
  dogs: number;
  horsemen: number;
  llamaRiders: number;
}

export interface BuildingDto {
  id: string;
  type: BuildingType;
  level: number;
}

export type MovementType = "Attack" | "Return" | "Settle" | "Transport";
export type MovementStatus = "InFlight" | "Recalled" | "Resolved";

export interface MovementDto {
  id: string;
  type: MovementType;
  status: MovementStatus;
  departureAt: string;
  arrivesAt: string;
  completedAt: string | null;
  troops: TroopsDto;
  carriedResources: ResourcesDto | null;
  originVillageId: string;
  originVillageName: string;
  targetVillageId: string | null;
  targetVillageName: string | null;
  targetCoordinates: Coordinates | null;
}

export interface BuildOrderDto {
  id: string;
  buildingType: BuildingType;
  targetLevel: number;
  startsAt: string;
  completesAt: string;
}

export interface TrainOrderDto {
  id: string;
  troopType: TroopType;
  amount: number;
  completed: number;
  startedAt: string;
  completesAt: string;
}

export interface VillageStatusDto {
  villageId: string;
  buildOrderCount: number;
  trainOrderCount: number;
  resources: ResourcesDto;
  troops: TroopsDto;
}

export interface VillageListItemDto {
  id: string;
  name: string;
  resources: ResourcesDto;
  troops: TroopsDto;
  coordinates: Coordinates;
}

export interface VillageDto {
  id: string;
  name: string;
  coordinates: Coordinates;
  resources: ResourcesDto;
  troops: TroopsDto;
  buildings: readonly BuildingDto[];
  buildOrders: readonly BuildOrderDto[];
  trainOrders: readonly TrainOrderDto[];
}

export interface PlayerVillageDto {
  id: string;
  playerId: string;
  name: string;
  playerName: string;
  coordinates: Coordinates;
  population: number;
  villageType: "Player" | "Barbarian";
}

export interface GetMapRequest {
  cords: Coordinates;
  radius: number;
}

export type ReportType =
  | "Attack"
  | "Defense"
  | "Settle"
  | "Return"
  | "Transport";

export interface ReportDto {
  id: string;
  type: ReportType;
  title: string;
  body: string;
  isRead: boolean;
  createdAt: string;
}

export interface ReportsResult {
  reports: ReportDto[];
  unreadCount: number;
  totalCount: number;
}

export interface LeaderboardEntryDto {
  playerId: string;
  username: string;
  score: number;
  rank: number;
}

export interface LeaderboardResult {
  items: LeaderboardEntryDto[];
  totalCount: number;
}

export interface TradeRequest {
  giveType: ResourceType;
  giveAmount: number;
  receiveType: ResourceType;
}

export type ResourceType = "Wood" | "Clay" | "Iron" | "Beer";

export interface AddResourcesRequest {
  wood: number;
  clay: number;
  iron: number;
  beer: number;
}

export type MapVillage =
  | ({ kind: "own" } & VillageListItemDto)
  | ({ kind: "enemy" } & PlayerVillageDto);
