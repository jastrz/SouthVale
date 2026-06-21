export interface TileData {
  terrain: "grass" | "water";
  decoration?: { kind: "tree" | "stone"; variant: number } | null;
  occupied: boolean;
  villageId?: string | null;
}
