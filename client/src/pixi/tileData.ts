export interface TileData {
  terrain: "grass" | "water";
  decoration?: { kind: "tree" | "bush"; variant: number } | null;
  occupied: boolean;
  villageId?: string | null;
}

/** Extract grid dimensions from a loaded TileData grid. */
export function gridSize(grid: TileData[][]): { cols: number; rows: number } {
  return { cols: grid[0]?.length ?? 0, rows: grid.length };
}
