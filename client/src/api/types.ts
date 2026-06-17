export type BuildingType =
  | "Warehouse"
  | "Granary"
  | "Barracks"
  | "IronMine"
  | "WoodCutter"
  | "CropField"
  | "ClayPit";

export type TroopType = "Swordsman" | "Archer" | "Settler";

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

export interface AuthResponse {
  accessToken: string;
}

export interface BuildRequest {
  buildingType: BuildingType;
}

export interface SettleRequest {
  targetX: number;
  targetY: number;
}

export interface TrainRequest {
  orders: TroopEntry[];
}

export interface AttackRequest {
  targetVillageId: string;
  troops: TroopEntry[];
}

export interface ResourcesDto {
  wood: number;
  clay: number;
  iron: number;
  crop: number;
}

export interface TroopsDto {
  swordsmen: number;
  archers: number;
  settlers: number;
}

export interface BuildingDto {
  id: string;
  type: BuildingType;
  level: number;
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

export interface VillageDto {
  id: string;
  name: string;
  mapX: number;
  mapY: number;
  resources: ResourcesDto;
  troops: TroopsDto;
  buildings: readonly BuildingDto[];
  buildOrders: readonly BuildOrderDto[];
  trainOrders: readonly TrainOrderDto[];
}
