export const BUILDING_ORDER = [
  "WoodCutter",
  "ClayPit",
  "IronMine",
  "CropField",
  "Warehouse",
  "Granary",
] as const;

export const BUILDING_LABELS: Record<string, string> = {
  WoodCutter: "Woodcutter",
  ClayPit: "Clay Pit",
  IronMine: "Iron Mine",
  CropField: "Crop Field",
  Warehouse: "Warehouse",
  Granary: "Granary",
  Barracks: "Barracks",
};

export const BUILDING_DESCRIPTIONS: Record<string, string> = {
  WoodCutter: "Produces wood",
  ClayPit: "Produces clay",
  IronMine: "Produces iron",
  CropField: "Produces crop",
  Warehouse: "Stores wood, clay, iron",
  Granary: "Stores crop",
  Barracks: "Trains troops",
};

export const TROOP_LABELS: Record<string, string> = {
  Swordsman: "Swordsman",
  Archer: "Archer",
  Settler: "Settler",
};
