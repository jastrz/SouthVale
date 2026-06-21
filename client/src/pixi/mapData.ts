export type Tile = 0 | 1;

/** Fill a rectangular region of the grid with a value. */
function fill(
  grid: Tile[][],
  x1: number,
  y1: number,
  x2: number,
  y2: number,
  val: Tile,
) {
  for (let y = y1; y <= y2; y++)
    for (let x = x1; x <= x2; x++) grid[y][x] = val;
}

/** 50x50 world map: 0 = grass, 1 = water. */
export const MAP_DATA: Tile[][] = Array.from({ length: 50 }, () =>
  Array(50).fill(0),
);

// Lake in the southeast connected to a river
fill(MAP_DATA, 32, 31, 40, 38, 1);
// Vertical river (north-south) from row 12 to row 34 at cols 23-24
fill(MAP_DATA, 23, 12, 24, 34, 1);
// Horizontal river section connecting to the lake
fill(MAP_DATA, 25, 33, 34, 34, 1);
// Small pond in the northwest
fill(MAP_DATA, 6, 6, 9, 9, 1);
