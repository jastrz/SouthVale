import { useState } from "react";
import type { UseMutationResult } from "@tanstack/react-query";
import type { AttackRequest, TroopEntry } from "../../api/types";
import { TravelEta } from "../travel-time/TravelEta";
import { useTravelTime } from "../travel-time/useTravelTime";

function TroopInput({
  label,
  value,
  onChange,
  max,
}: {
  label: string;
  value: number;
  onChange: (v: number) => void;
  max: number;
}) {
  return (
    <div className="flex-1">
      <label className="block text-[10px] text-slate-400">{label}</label>
      <input
        type="number"
        min={0}
        max={max}
        value={value || ""}
        onChange={(e) =>
          onChange(Math.max(0, Number.parseInt(e.target.value) || 0))
        }
        className="w-full rounded border border-slate-600 bg-slate-900 px-1.5 py-1 text-xs text-white outline-none focus:border-red-500"
        placeholder="0"
      />
      <button
        type="button"
        onClick={() => onChange(max)}
        className="mt-0.5 w-full cursor-pointer rounded bg-slate-700/60 px-1 py-px text-[9px] text-slate-400 transition-colors hover:bg-slate-600/60 hover:text-slate-200"
      >
        Max: {max}
      </button>
    </div>
  );
}

export function AttackPanel({
  targetName,
  targetX,
  targetY,
  targetPopulation,
  targetVillageId,
  maxSwordsmen,
  maxArchers,
  mutation,
  onClearTarget,
}: {
  targetName: string;
  targetX: number;
  targetY: number;
  targetPopulation: number;
  targetVillageId: string;
  maxSwordsmen: number;
  maxArchers: number;
  mutation: UseMutationResult<unknown, unknown, AttackRequest, unknown>;
  onClearTarget: () => void;
}) {
  const [swordsmen, setSwordsmen] = useState(0);
  const [archers, setArchers] = useState(0);
  const { getSpeed } = useTravelTime();

  const selected = [
    { type: "Swordsman" as const, count: swordsmen },
    { type: "Archer" as const, count: archers },
  ].filter((t) => t.count > 0);

  const speed =
    selected.length > 0
      ? Math.min(...selected.map((t) => getSpeed(t.type)))
      : Math.min(getSpeed("Swordsman"), getSpeed("Archer"));

  const handleAttack = () => {
    if (swordsmen === 0 && archers === 0) return;
    const troops: TroopEntry[] = [];
    if (swordsmen > 0)
      troops.push({ troopType: "Swordsman", count: swordsmen });
    if (archers > 0) troops.push({ troopType: "Archer", count: archers });
    mutation.mutate({ troops, targetVillageId });
  };

  return (
    <section className="border-t border-red-900/60 bg-red-950/20 px-4 py-3">
      <div className="mb-2 flex items-center justify-between">
        <h3 className="text-xs font-bold tracking-widest text-red-400 uppercase">
          Attack Target
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
        <div className="font-medium text-white">{targetName}</div>
        <div className="text-slate-400">
          ({targetX}, {targetY}) · Population: {targetPopulation}
        </div>
        <TravelEta toX={targetX} toY={targetY} speed={speed} />
      </div>

      <div className="flex flex-col gap-3">
        <div className="flex gap-1">
          <TroopInput
            label="Swordsmen"
            value={swordsmen}
            onChange={setSwordsmen}
            max={maxSwordsmen}
          />
          <TroopInput
            label="Archers"
            value={archers}
            onChange={setArchers}
            max={maxArchers}
          />
        </div>

        <button
          type="button"
          onClick={handleAttack}
          disabled={(swordsmen === 0 && archers === 0) || mutation.isPending}
          className="w-full cursor-pointer rounded bg-red-700 px-3 py-1.5 text-xs font-medium text-white transition-colors hover:bg-red-600 disabled:cursor-not-allowed disabled:opacity-40 disabled:hover:bg-red-700"
        >
          {mutation.isPending ? "Sending…" : "Send Attack"}
        </button>
      </div>
    </section>
  );
}
