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
