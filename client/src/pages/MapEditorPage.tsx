import { useEffect, useRef, useState } from "react";
import { createApplication } from "../pixi/app";
import { EditorScene } from "../pixi/editor/EditorScene";
import type { Tool } from "../pixi/editor/EditorScene";
import { loadMap } from "../pixi/mapLoader";
import type { TileData } from "../pixi/tileData";

const TOOLS: Tool[] = ["grass", "water", "tree", "bush", "erase"];

export function MapEditorPage() {
  const divRef = useRef<HTMLDivElement>(null);
  const sceneRef = useRef<EditorScene | null>(null);
  const [tool, setTool] = useState<Tool>("grass");
  const [width, setWidth] = useState(50);
  const [height, setHeight] = useState(50);

  useEffect(() => {
    let scene: EditorScene | null = null;
    (async () => {
      const [app, grid] = await Promise.all([
        createApplication(divRef.current!),
        loadMap("/maps/default.json"),
      ]);
      scene = new EditorScene(app, grid);
      sceneRef.current = scene;
      setWidth(scene.cols);
      setHeight(scene.rows);
    })();
    return () => {
      scene?.destroy();
      sceneRef.current = null;
    };
  }, []);

  useEffect(() => {
    sceneRef.current?.setTool(tool);
  }, [tool]);

  const handleResize = () => {
    const w = Math.max(1, Math.min(200, width));
    const h = Math.max(1, Math.min(200, height));
    setWidth(w);
    setHeight(h);
    sceneRef.current?.resize(w, h);
  };

  const handleSave = () => {
    const grid = sceneRef.current?.getGrid();
    if (!grid) return;
    const rows = grid.length;
    const cols = grid[0].length;
    const terrain = grid
      .map((row) =>
        row.map((td) => (td.terrain === "water" ? "W" : "G")).join(""),
      )
      .join("");
    const decorations: [number, number, number, string][] = [];
    for (let y = 0; y < rows; y++)
      for (let x = 0; x < cols; x++) {
        const d = grid[y][x].decoration;
        if (d) decorations.push([x, y, d.variant, d.kind]);
      }
    const blob = new Blob(
      [JSON.stringify({ cols, rows, terrain, decorations }, null, 2)],
      { type: "application/json" },
    );
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = "map.json";
    a.click();
    URL.revokeObjectURL(url);
  };

  const handleLoad = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = () => {
      try {
        const data = JSON.parse(reader.result as string);
        const rows = data.rows ?? 50;
        const cols = data.cols ?? 50;
        const grid: TileData[][] = [];
        for (let y = 0; y < rows; y++) {
          grid[y] = [];
          for (let x = 0; x < cols; x++) {
            const ch = data.terrain[y * cols + x];
            grid[y][x] = {
              terrain: ch === "W" ? "water" : "grass",
              decoration: null,
              occupied: false,
              villageId: null,
            };
          }
        }
        for (const entry of data.decorations ?? data.trees ?? []) {
          const [x, y, variant, kind] = entry;
          if (y < rows && x < cols)
            grid[y][x].decoration = { kind: kind ?? "tree", variant };
        }
        sceneRef.current?.loadGrid(grid);
        setWidth(cols);
        setHeight(rows);
      } catch {
        alert("Invalid map JSON");
      }
    };
    reader.readAsText(file);
  };

  const handleResetZoom = () => {
    const scene = sceneRef.current;
    if (!scene) return;
    scene.root.scale.set(1);
    scene.root.x = 0;
    scene.root.y = 0;
  };

  return (
    <div className="flex flex-col bg-slate-900" style={{ height: "100dvh" }}>
      <div className="flex items-center gap-2 border-b border-slate-700 px-4 py-2">
        {TOOLS.map((t) => (
          <button
            key={t}
            onClick={() => setTool(t)}
            className={`cursor-pointer rounded px-3 py-1 text-sm font-medium capitalize ${
              tool === t
                ? "bg-blue-600 text-white"
                : "bg-slate-700 text-slate-300 hover:bg-slate-600"
            }`}
          >
            {t}
          </button>
        ))}
        <span className="mx-2 h-6 w-px bg-slate-600" />
        <label className="text-sm text-slate-400">W:</label>
        <input
          type="number"
          min={1}
          max={200}
          value={width}
          onChange={(e) => setWidth(Number(e.target.value))}
          className="w-16 rounded bg-slate-700 px-2 py-1 text-sm text-white"
        />
        <label className="text-sm text-slate-400">H:</label>
        <input
          type="number"
          min={1}
          max={200}
          value={height}
          onChange={(e) => setHeight(Number(e.target.value))}
          className="w-16 rounded bg-slate-700 px-2 py-1 text-sm text-white"
        />
        <button
          onClick={handleResize}
          className="cursor-pointer rounded bg-amber-700 px-3 py-1 text-sm font-medium text-white hover:bg-amber-600"
        >
          Resize
        </button>
        <span className="mx-2 h-6 w-px bg-slate-600" />
        <button
          onClick={handleSave}
          className="cursor-pointer rounded bg-green-700 px-3 py-1 text-sm font-medium text-white hover:bg-green-600"
        >
          Save JSON
        </button>
        <label className="cursor-pointer rounded bg-slate-700 px-3 py-1 text-sm font-medium text-slate-300 hover:bg-slate-600">
          Load JSON
          <input
            type="file"
            accept=".json"
            onChange={handleLoad}
            className="hidden"
          />
        </label>
        <button
          onClick={handleResetZoom}
          className="cursor-pointer rounded bg-slate-700 px-3 py-1 text-sm font-medium text-slate-300 hover:bg-slate-600"
        >
          Reset zoom
        </button>
      </div>
      <div ref={divRef} className="flex-1 min-h-0" />
    </div>
  );
}
