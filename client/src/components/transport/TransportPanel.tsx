import { useState } from "react";
import type { UseMutationResult } from "@tanstack/react-query";
import type {
  TransportRequest,
  TroopEntry,
  ResourcesDto,
} from "../../api/types";
import { TravelEta } from "../travel-time/TravelEta";
import { useTravelTime } from "../travel-time/useTravelTime";
import { useMyVillages, useGameConfig } from "../../api/hooks/useQueries";
import { NumberInput } from "../NumberInput";

export function TransportPanel({
  villageId,
  troops,
  resources,
  mutation,
  onClose,
}: {
  villageId: string;
  troops: { swordsmen: number; archers: number; settlers: number };
  resources: ResourcesDto;
  mutation: UseMutationResult<unknown, Error, TransportRequest, unknown>;
  onClose: () => void;
}) {
  const { data: myVillages } = useMyVillages();
  const { data: gameConfig } = useGameConfig();
  const { getSpeed } = useTravelTime();

  const targets = myVillages?.filter((v) => v.id !== villageId) ?? [];

  const [targetVillageId, setTargetVillageId] = useState("");
  const [swordsmen, setSwordsmen] = useState(0);
  const [archers, setArchers] = useState(0);
  const [settlers, setSettlers] = useState(0);
  const [wood, setWood] = useState(0);
  const [clay, setClay] = useState(0);
  const [iron, setIron] = useState(0);
  const [crop, setCrop] = useState(0);

  const selectedVillage = targets.find((v) => v.id === targetVillageId);
  const carryCapacity =
    swordsmen * (gameConfig?.troops.Swordsman?.carryCapacity ?? 0) +
    archers * (gameConfig?.troops.Archer?.carryCapacity ?? 0) +
    settlers * (gameConfig?.troops.Settler?.carryCapacity ?? 0);
  const resourcesUsed = wood + clay + iron + crop;
  const maxFor = (have: number, key: "wood" | "clay" | "iron" | "crop") => {
    if (!carryCapacity) return 0;
    const others = resourcesUsed - { wood, clay, iron, crop }[key];
    const remaining = carryCapacity - others;
    return Math.min(Math.floor(have), Math.max(0, remaining));
  };
  const hasResources = wood > 0 || clay > 0 || iron > 0 || crop > 0;
  const hasTroops = swordsmen > 0 || archers > 0 || settlers > 0;

  const speed = hasTroops
    ? Math.min(
        ...[
          { type: "Swordsman" as const, count: swordsmen },
          { type: "Archer" as const, count: archers },
          { type: "Settler" as const, count: settlers },
        ]
          .filter((t) => t.count > 0)
          .map((t) => getSpeed(t.type)),
      )
    : getSpeed("Swordsman");

  const handleSend = () => {
    if (!targetVillageId || (!hasTroops && !hasResources)) return;
    const troopEntries: TroopEntry[] = [];
    if (swordsmen > 0)
      troopEntries.push({ troopType: "Swordsman", count: swordsmen });
    if (archers > 0) troopEntries.push({ troopType: "Archer", count: archers });
    if (settlers > 0)
      troopEntries.push({ troopType: "Settler", count: settlers });
    mutation.mutate(
      {
        targetVillageId,
        troops: troopEntries,
        resources: { wood, clay, iron, crop },
      },
      { onSuccess: onClose },
    );
  };

  return (
    <section className="flex flex-col gap-3 border-t border-emerald-900/60 bg-emerald-950/20 px-4 py-4">
      <div className="flex items-center justify-between">
        <h3 className="text-xs font-bold tracking-widest text-emerald-400 uppercase">
          Transport
        </h3>
        <button
          type="button"
          onClick={onClose}
          className="cursor-pointer text-[11px] text-slate-500 hover:text-slate-300"
        >
          Close
        </button>
      </div>

      <div>
        <label className="block text-[10px] text-slate-400">
          Target Village
        </label>
        <select
          value={targetVillageId}
          onChange={(e) => setTargetVillageId(e.target.value)}
          className="w-full rounded border border-slate-600 bg-slate-900 px-1.5 py-1 text-xs text-white outline-none focus:border-emerald-500"
        >
          <option value="">Select a village…</option>
          {targets.map((v) => (
            <option key={v.id} value={v.id}>
              {v.name} ({v.coordinates.x}, {v.coordinates.y})
            </option>
          ))}
        </select>
      </div>

      {selectedVillage && (
        <div className="rounded bg-slate-800/40 px-2.5 py-1.5 text-xs">
          {/*<div className="text-slate-400">
            Target: {selectedVillage.name} ({selectedVillage.coordinates.x},{" "}
            {selectedVillage.coordinates.y})
          </div>*/}
          <TravelEta
            toX={selectedVillage.coordinates.x}
            toY={selectedVillage.coordinates.y}
            speed={speed}
          />
        </div>
      )}

      <div>
        <div className="mb-1 text-[10px] font-medium text-slate-400">
          Troops
        </div>
        <div className="flex gap-1">
          <div className="flex-1">
            <label className="block text-[10px] text-slate-400">
              Swordsmen
            </label>
            <NumberInput
              value={swordsmen}
              onChange={setSwordsmen}
              max={troops.swordsmen}
            />
          </div>
          <div className="flex-1">
            <label className="block text-[10px] text-slate-400">Archers</label>
            <NumberInput
              value={archers}
              onChange={setArchers}
              max={troops.archers}
            />
          </div>
          <div className="flex-1">
            <label className="block text-[10px] text-slate-400">Settlers</label>
            <NumberInput
              value={settlers}
              onChange={setSettlers}
              max={troops.settlers}
            />
          </div>
        </div>
      </div>

      <div>
        <div className="mb-1 flex items-center justify-between text-[12px]">
          <span className="font-medium text-slate-400">Resources</span>
          {carryCapacity > 0 && (
            <span
              className={
                resourcesUsed > carryCapacity
                  ? "font-medium text-red-400"
                  : "text-slate-200"
              }
            >
              {resourcesUsed} / {carryCapacity}
            </span>
          )}
        </div>
        <div className="border-t border-slate-700 pt-1" />

        <div className="flex gap-1">
          <div className="flex-1">
            <label className="block text-[10px] text-slate-400">Wood</label>
            <NumberInput
              value={wood}
              onChange={setWood}
              max={maxFor(resources.wood, "wood")}
            />
          </div>
          <div className="flex-1">
            <label className="block text-[10px] text-slate-400">Clay</label>
            <NumberInput
              value={clay}
              onChange={setClay}
              max={maxFor(resources.clay, "clay")}
            />
          </div>
          <div className="flex-1">
            <label className="block text-[10px] text-slate-400">Iron</label>
            <NumberInput
              value={iron}
              onChange={setIron}
              max={maxFor(resources.iron, "iron")}
            />
          </div>
          <div className="flex-1">
            <label className="block text-[10px] text-slate-400">Crop</label>
            <NumberInput
              value={crop}
              onChange={setCrop}
              max={maxFor(resources.crop, "crop")}
            />
          </div>
        </div>
      </div>

      <button
        type="button"
        onClick={handleSend}
        disabled={
          !targetVillageId ||
          (!hasTroops && !hasResources) ||
          mutation.isPending
        }
        className="w-full cursor-pointer rounded bg-emerald-700 px-3 py-1.5 text-xs font-medium text-white transition-colors hover:bg-emerald-600 disabled:cursor-not-allowed disabled:opacity-40 disabled:hover:bg-emerald-700"
      >
        {mutation.isPending ? "Sending…" : "Send Transport"}
      </button>
      {mutation.isError && (
        <p className="mt-1 text-xs text-red-400">
          {mutation.error instanceof Error
            ? mutation.error.message
            : "Transport failed"}
        </p>
      )}
    </section>
  );
}
