import { useGameStateStore } from "../store/gameStateStore";
import { useVillage, useSettle, useMyVillages, useGameConfig } from "../api/hooks/useQueries";
import { Modal } from "./Modal";
import { travelTime } from "../lib/helpers";

export function TilePopup() {
  const selectedTile = useGameStateStore((s) => s.selectedTile);
  const setSelectedTile = useGameStateStore((s) => s.setSelectedTile);
  const activeVillageId = useGameStateStore((s) => s.activeVillageId);
  const { data: village } = useVillage(activeVillageId ?? "");
  const { data: myVillages } = useMyVillages();
  const { data: config } = useGameConfig();
  const mutation = useSettle(activeVillageId ?? "");

  if (!selectedTile || !village) return null;

  const { x, y, tile } = selectedTile;
  const settlers = village.troops.settlers;
  const villageCount = myVillages?.length ?? 1;
  const maxVillages = config?.maxVillagesPerPlayer ?? 8;
  const canSettle = tile.terrain === "grass" && !tile.occupied && !tile.decoration;

  return (
    <Modal open={!!selectedTile} onClose={() => setSelectedTile(null)}>
      <div className="flex flex-col gap-3 p-4">
        <div className="flex items-center justify-between">
          <h3 className="text-xs font-bold tracking-widest text-slate-300 uppercase">Tile Info</h3>
          <button type="button" onClick={() => setSelectedTile(null)} className="cursor-pointer text-xs text-slate-500 hover:text-slate-300">✕</button>
        </div>
        <div className="rounded bg-slate-800/40 px-2.5 py-1.5 text-xs space-y-1">
          <div className="text-slate-300">Coordinates: ({x}, {y})</div>
          <div className="text-slate-400">Terrain: <span className={tile.terrain === "water" ? "text-blue-400" : "text-green-400"}>{tile.terrain}</span></div>
          {tile.decoration && <div className="text-slate-400">Obstacle: {tile.decoration.kind}</div>}
          {tile.occupied && <div className="text-yellow-400">Occupied by another village</div>}
          {!canSettle && !tile.occupied && <div className="text-red-400">Cannot settle here</div>}
        </div>
        {canSettle && (
          <div className="rounded bg-slate-800/40 px-2.5 py-1.5 text-xs space-y-1">
            <div className="text-slate-400">Settlers available: {settlers}</div>
            <div className="text-slate-400">Villages: {villageCount} / {maxVillages}</div>
            <div className="text-yellow-400">time: {travelTime(village.coordinates.x, village.coordinates.y, x, y, 5, config?.travelSpeedMultiplier ?? 10)}</div>
            <button
              type="button"
              onClick={() => mutation.mutate({ target: { x, y } }, { onSuccess: () => setSelectedTile(null) })}
              disabled={settlers < 1 || mutation.isPending}
              className="w-full cursor-pointer rounded bg-emerald-700 px-3 py-1.5 text-xs font-medium text-white transition-colors hover:bg-emerald-600 disabled:cursor-not-allowed disabled:opacity-40"
            >
              {mutation.isPending ? "Sending…" : settlers < 1 ? "No settlers" : "Send Settler"}
            </button>
          </div>
        )}
      </div>
    </Modal>
  );
}
