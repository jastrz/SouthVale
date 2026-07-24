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
  maxDogs,
  maxHorsemen,
  maxLlamaRiders,
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
  maxDogs: number;
  maxHorsemen: number;
  maxLlamaRiders: number;
  mutation: UseMutationResult<unknown, unknown, AttackRequest, unknown>;
  onClearTarget: () => void;
}) {
  const [swordsmen, setSwordsmen] = useState(0);
  const [archers, setArchers] = useState(0);
  const [dogs, setDogs] = useState(0);
  const [horsemen, setHorsemen] = useState(0);
  const [llamaRiders, setLlamaRiders] = useState(0);
  const { getSpeed } = useTravelTime();

  const selected = [
    { type: "Swordsman" as const, count: swordsmen },
    { type: "Archer" as const, count: archers },
    { type: "Dogs" as const, count: dogs },
    { type: "Horsemen" as const, count: horsemen },
    { type: "LlamaRiders" as const, count: llamaRiders },
  ].filter((t) => t.count > 0);

  const speed =
    selected.length > 0
      ? Math.min(...selected.map((t) => getSpeed(t.type)))
      : Math.min(getSpeed("Swordsman"), getSpeed("Archer"), getSpeed("Dogs"), getSpeed("Horsemen"), getSpeed("LlamaRiders"), getSpeed("Settler"));

  const handleAttack = () => {
    if (selected.length === 0) return;
    const troops: TroopEntry[] = selected.map((t) => ({ troopType: t.type, count: t.count }));
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

      <div className="flex gap-1 flex-wrap">
        <div className="grid grid-cols-3 gap-1">
            {maxSwordsmen > 0 && <TroopInput label="Swordsmen" value={swordsmen} onChange={setSwordsmen} max={maxSwordsmen} />}
            {maxArchers > 0 && <TroopInput label="Archers" value={archers} onChange={setArchers} max={maxArchers} />}
            {maxDogs > 0 && <TroopInput label="Dogs" value={dogs} onChange={setDogs} max={maxDogs} />}
            {maxHorsemen > 0 && <TroopInput label="Horsemen" value={horsemen} onChange={setHorsemen} max={maxHorsemen} />}
            {maxLlamaRiders > 0 && <TroopInput label="LlamaRiders" value={llamaRiders} onChange={setLlamaRiders} max={maxLlamaRiders} />}
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
