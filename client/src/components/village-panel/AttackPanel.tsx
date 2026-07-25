import { useState } from "react";
import type { UseMutationResult } from "@tanstack/react-query";
import type { AttackRequest, TroopEntry } from "../../api/types";
import { TravelEta } from "../travel-time/TravelEta";
import { useTravelTime } from "../travel-time/useTravelTime";
import { NumberInput } from "../NumberInput";

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
            {maxSwordsmen > 0 && <div className="flex-1"><NumberInput label="Swordsmen" value={swordsmen} onChange={setSwordsmen} max={maxSwordsmen} /></div>}
            {maxArchers > 0 && <div className="flex-1"><NumberInput label="Archers" value={archers} onChange={setArchers} max={maxArchers} /></div>}
            {maxDogs > 0 && <div className="flex-1"><NumberInput label="Dogs" value={dogs} onChange={setDogs} max={maxDogs} /></div>}
            {maxHorsemen > 0 && <div className="flex-1"><NumberInput label="Horsemen" value={horsemen} onChange={setHorsemen} max={maxHorsemen} /></div>}
            {maxLlamaRiders > 0 && <div className="flex-1"><NumberInput label="LlamaRiders" value={llamaRiders} onChange={setLlamaRiders} max={maxLlamaRiders} /></div>}
        </div>

        <button
          type="button"
          onClick={handleAttack}
          disabled={selected.length === 0 || mutation.isPending}
          className="w-full cursor-pointer rounded bg-red-700 px-3 py-1.5 text-xs font-medium text-white transition-colors hover:bg-red-600 disabled:cursor-not-allowed disabled:opacity-40 disabled:hover:bg-red-700"
        >
          {mutation.isPending ? "Sending…" : "Send Attack"}
        </button>
      </div>
    </section>
  );
}
