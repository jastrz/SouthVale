export const BUILDING_ORDER = [
  "TownHall",
  "WoodCutter",
  "ClayPit",
  "IronMine",
  "Brewery",
  "Warehouse",
  "Barracks",
  "Stable",
  "TradePost",
  "Cranny",
  "Wall",

] as const;

export const BUILDING_LABELS: Record<string, string> = {
  WoodCutter: "Woodcutter",
  ClayPit: "Clay Pit",
  IronMine: "Iron Mine",
  Brewery: "Brewery",
  Warehouse: "Warehouse",
  Barracks: "Barracks",
  Stable: "Stable",
  Wall: "Wall",
  Cranny: "Cranny",
  TradePost: "Trade Post",
  TownHall: "Town Hall",
};

export const BUILDING_DESCRIPTIONS: Record<string, string> = {
  WoodCutter: "Produces wood",
  ClayPit: "Produces clay",
  IronMine: "Produces iron",
  Brewery: "Produces beer",
  Warehouse: "Stores all resources",
  Barracks: "Trains infantry",
  Stable: "Trains cavalry",
  Wall: "Defends village",
  Cranny: "Hides resources from attackers",
  TradePost: "Improves trade rates",
  TownHall: "Speeds up construction",
};

export const TROOP_LABELS: Record<string, string> = {
  Swordsman: "Swordsman",
  Archer: "Archer",
  Settler: "Settler",
  Dogs: "Dogs",
  Horsemen: "Horsemen",
  LlamaRiders: "Llama Riders",
};
