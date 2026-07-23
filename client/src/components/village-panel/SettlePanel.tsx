import type { UseMutationResult } from "@tanstack/react-query";
import type { SettleRequest } from "../../api/types";
import { TravelEta } from "../travel-time/TravelEta";
import { useTravelTime } from "../travel-time/useTravelTime";

export function SettlePanel({
  targetX,
  targetY,
  settlers,
  villageCount,
  maxVillages,
  mutation,
  onClearTarget,
}: {
  targetX: number;
  targetY: number;
  settlers: number;
  villageCount: number;
  maxVillages: number;
  mutation: UseMutationResult<unknown, unknown, SettleRequest, unknown>;
  onClearTarget: () => void;
}) {
  const { getSpeed } = useTravelTime();

  return (
    <section className="border-t border-emerald-900/60 bg-emerald-950/20 px-4 py-3">
      <div className="mb-2 flex items-center justify-between">
        <h3 className="text-xs font-bold tracking-widest text-emerald-400 uppercase">
          Settle
        </h3>
        <button
          type="button"
          onClick={onClearTarget}
          className="cursor-pointer text-[11px] text-slate-500 hover:text-slate-300"
        >
          Clear
        </button>
      </div>

      <div className="mb-2 rounded bg-slate-800/40 px-2.5 py-1.5 text-xs">
        <div className="text-slate-400">
          Target: ({targetX}, {targetY})
        </div>
        <div className="text-slate-400">
          Settlers available: {settlers}
        </div>
        <div className="text-slate-400">
          Villages: {villageCount} / {maxVillages}
        </div>
        <TravelEta toX={targetX} toY={targetY} speed={getSpeed("Settler")} />
      </div>

      <button
        type="button"
        onClick={() => mutation.mutate({ target: { x: targetX, y: targetY } })}
        disabled={settlers < 1 || mutation.isPending}
        className="w-full cursor-pointer rounded bg-emerald-700 px-3 py-1.5 text-xs font-medium text-white transition-colors hover:bg-emerald-600 disabled:cursor-not-allowed disabled:opacity-40 disabled:hover:bg-emerald-700"
      >
        {mutation.isPending ? "Sending…" : settlers < 1 ? "No settlers" : "Send Settler"}
      </button>
    </section>
  );
}
