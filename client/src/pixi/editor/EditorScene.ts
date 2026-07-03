import { Container, Sprite, Rectangle, Point } from "pixi.js";
import type { Application, FederatedPointerEvent } from "pixi.js";
import { TILE_SIZE } from "../config";
import { groundTile, treeTile, ATLAS_GROUND, ATLAS_TREES } from "../atlas";
import { autotile } from "../autotile";
import { createTerrainTile } from "../entities/TerrainTile";
import { attachZoom } from "../input";
import { type TileData, gridSize } from "../tileData";

export type Tool = "grass" | "water" | "tree" | "erase";

const BUSH_TILES = [
  ATLAS_TREES.BUSH_RED,
  ATLAS_TREES.BUSH_YELLOW,
  ATLAS_TREES.BUSH_LIGHT_GREEN,
  ATLAS_TREES.BUSH_DARK_GREEN,
] as const;

const TREE_TILES = [
  ATLAS_TREES.TREE_RED,
  ATLAS_TREES.TREE_YELLOW,
  ATLAS_TREES.TREE_DARK_GREEN,
  ATLAS_TREES.TREE_LIGHT_GREEN,
  ATLAS_TREES.TREE2_DARK_GREEN,
  ATLAS_TREES.TREE2_LIGHT_GREEN,
] as const;

function pick(pool: readonly (readonly [number, number])[]) {
  return pool[Math.floor(Math.random() * pool.length)];
}

/**
 * Tile-map editor scene. Left-click paints with the selected tool, right-drag
 * pans, scroll zooms.
 */
export class EditorScene {
  readonly root = new Container();
  cols: number;
  rows: number;
  private terrainSprites: Sprite[][] = [];
  private decorSprites: (Sprite | null)[][] = [];
  private grid: TileData[][];
  private tool: Tool = "grass";
  private painting = false;
  private panning = false;
  private lastX = -1;
  private lastY = -1;
  private panStart = new Point();
  private panOrigin = new Point();
  private disposers: (() => void)[] = [];

  constructor(app: Application, grid: TileData[][]) {
    this.grid = grid;
    this.cols = grid[0]?.length ?? 50;
    this.rows = grid.length;
    this.root.label = "EditorRoot";
    this.root.eventMode = "static";
    this.root.hitArea = new Rectangle(
      0,
      0,
      this.cols * TILE_SIZE,
      this.rows * TILE_SIZE,
    );
    app.stage.addChild(this.root);
    this.render();
    this.attachEvents(app);
    this.centerView(app);
  }

  setTool(tool: Tool): void {
    this.tool = tool;
  }
  getGrid(): TileData[][] {
    return this.grid;
  }

  loadGrid(grid: TileData[][]): void {
    this.grid = grid;
    const sz = gridSize(grid);
    this.cols = sz.cols;
    this.rows = sz.rows;
    this.destroyAll();
    this.render();
  }

  resize(cols: number, rows: number): void {
    if (cols === this.cols && rows === this.rows) return;
    const newGrid: TileData[][] = [];
    for (let y = 0; y < rows; y++) {
      newGrid[y] = [];
      for (let x = 0; x < cols; x++) {
        if (y < this.rows && x < this.cols) {
          const src = this.grid[y][x];
          newGrid[y][x] = {
            terrain: src.terrain,
            decoration: src.decoration ? { ...src.decoration } : null,
            occupied: src.occupied,
            villageId: src.villageId,
          };
        } else {
          newGrid[y][x] = {
            terrain: "grass",
            decoration: null,
            occupied: false,
            villageId: null,
          };
        }
      }
    }
    this.grid = newGrid;
    this.cols = cols;
    this.rows = rows;
    this.destroyAll();
    this.render();
    this.root.hitArea = new Rectangle(0, 0, cols * TILE_SIZE, rows * TILE_SIZE);
  }

  destroy(): void {
    this.painting = false;
    this.panning = false;
    this.disposers.forEach((fn) => fn());
    this.disposers = [];
    this.destroyAll();
    this.root.removeFromParent();
    this.root.destroy({ children: true });
  }

  // ---- internal ----

  private attachEvents(app: Application): void {
    const canvas = app.canvas;

    // Left-click painting via Pixi federated events
    this.root.on("pointerdown", (e: FederatedPointerEvent) => {
      if (e.button !== 0) return;
      this.painting = true;
      this.paintAt(e);
    });
    this.root.on("pointermove", (e: FederatedPointerEvent) => {
      if (this.painting && e.buttons & 1) this.paintAt(e);
    });
    const stopPaint = () => {
      this.painting = false;
      this.lastX = -1;
      this.lastY = -1;
    };
    this.root.on("pointerup", stopPaint);
    this.root.on("pointerupoutside", stopPaint);
    canvas.addEventListener("pointerup", stopPaint);
    canvas.addEventListener("pointerleave", stopPaint);

    // Right-click drag pan via native events
    const onMouseDown = (e: MouseEvent) => {
      if (e.button !== 2) return;
      this.panning = true;
      this.panStart.set(e.clientX, e.clientY);
      this.panOrigin.set(this.root.x, this.root.y);
    };
    const onMouseMove = (e: MouseEvent) => {
      if (!this.panning) return;
      this.root.x = this.panOrigin.x + (e.clientX - this.panStart.x);
      this.root.y = this.panOrigin.y + (e.clientY - this.panStart.y);
    };
    const stopPan = () => {
      this.panning = false;
    };
    canvas.addEventListener("mousedown", onMouseDown);
    window.addEventListener("mousemove", onMouseMove);
    window.addEventListener("mouseup", stopPan);
    canvas.addEventListener("contextmenu", (e) => e.preventDefault());

    // Scroll-wheel zoom (reuses game zoom controller)
    const disposeZoom = attachZoom(canvas, this.root);
    this.disposers.push(disposeZoom);
    this.disposers.push(() => {
      canvas.removeEventListener("mousedown", onMouseDown);
      window.removeEventListener("mousemove", onMouseMove);
      window.removeEventListener("mouseup", stopPan);
    });
  }

  private tileFromEvent(
    e: FederatedPointerEvent,
  ): { x: number; y: number } | null {
    const local = e.getLocalPosition(this.root);
    const x = Math.floor(local.x / TILE_SIZE);
    const y = Math.floor(local.y / TILE_SIZE);
    if (x < 0 || x >= this.cols || y < 0 || y >= this.rows) return null;
    return { x, y };
  }

  private paintAt(e: FederatedPointerEvent): void {
    const pos = this.tileFromEvent(e);
    if (!pos) return;
    if (pos.x === this.lastX && pos.y === this.lastY) return;
    this.lastX = pos.x;
    this.lastY = pos.y;
    this.applyTool(pos.x, pos.y);
  }

  private applyTool(x: number, y: number): void {
    const td = this.grid[y][x];
    switch (this.tool) {
      case "grass":
        td.terrain = "grass";
        td.decoration = null;
        break;
      case "water":
        td.terrain = "water";
        td.decoration = null;
        break;
      case "tree":
        td.decoration = {
          kind: "tree",
          variant: Math.floor(Math.random() * 5) + 1,
        };
        if (td.decoration.variant <= 2) td.decoration.kind = "bush";
        break;
      case "erase":
        td.decoration = null;
        break;
    }
    this.refreshTile(x, y);
    for (let dy = -1; dy <= 1; dy++)
      for (let dx = -1; dx <= 1; dx++) {
        if (dx === 0 && dy === 0) continue;
        const nx = x + dx,
          ny = y + dy;
        if (nx >= 0 && nx < this.cols && ny >= 0 && ny < this.rows)
          this.refreshTerrain(nx, ny);
      }
  }

  private render(): void {
    this.terrainSprites = [];
    this.decorSprites = [];
    for (let y = 0; y < this.rows; y++) {
      this.terrainSprites[y] = [];
      this.decorSprites[y] = [];
      for (let x = 0; x < this.cols; x++) {
        this.placeTerrain(x, y);
        this.placeDecoration(x, y);
      }
    }
  }

  private placeTerrain(x: number, y: number): void {
    const td = this.grid[y][x];
    const sprite = createTerrainTile(
      x, y,
      td.terrain === "water"
        ? groundTile(...ATLAS_GROUND.WATER)
        : groundTile(...autotile(this.grid, x, y)),
    );
    this.root.addChild(sprite);
    this.terrainSprites[y][x] = sprite;
  }

  private placeDecoration(x: number, y: number): void {
    const deco = this.grid[y][x]?.decoration;
    if (!deco) { this.decorSprites[y][x] = null; return; }
    const pool = deco.kind === "tree" ? TREE_TILES : deco.kind === "bush" ? BUSH_TILES : null;
    if (!pool) { this.decorSprites[y][x] = null; return; }
    const sprite = createTerrainTile(x, y, treeTile(...pick(pool)));
    this.root.addChild(sprite);
    this.decorSprites[y][x] = sprite;
  }

  private destroyAll(): void {
    for (let y = 0; y < this.rows; y++) {
      for (let x = 0; x < this.cols; x++) {
        this.terrainSprites[y]?.[x]?.destroy();
        this.decorSprites[y]?.[x]?.destroy();
      }
    }
  }

  private refreshTile(x: number, y: number): void {
    const td = this.grid[y][x];
    this.terrainSprites[y][x].texture =
      td.terrain === "water"
        ? groundTile(...ATLAS_GROUND.WATER)
        : groundTile(...autotile(this.grid, x, y));
    const existing = this.decorSprites[y][x];
    const deco = td.decoration;
    const pool = deco?.kind === "tree" ? TREE_TILES : deco?.kind === "bush" ? BUSH_TILES : null;
    if (pool) {
      if (existing) {
        existing.texture = treeTile(...pick(pool));
      } else {
        const sprite = createTerrainTile(x, y, treeTile(...pick(pool)));
        this.root.addChild(sprite);
        this.decorSprites[y][x] = sprite;
      }
    } else {
      if (existing) {
        existing.removeFromParent();
        existing.destroy();
        this.decorSprites[y][x] = null;
      }
    }
  }

  private refreshTerrain(x: number, y: number): void {
    const td = this.grid[y][x];
    this.terrainSprites[y][x].texture =
      td.terrain === "water"
        ? groundTile(...ATLAS_GROUND.WATER)
        : groundTile(...autotile(this.grid, x, y));
  }

  private centerView(app: Application): void {
    this.root.x = app.screen.width / 2 - (this.cols * TILE_SIZE) / 2;
    this.root.y = app.screen.height / 2 - (this.rows * TILE_SIZE) / 2;
  }
}
