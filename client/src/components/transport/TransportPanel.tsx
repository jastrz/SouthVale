import { useState } from "react";
import type { UseMutationResult } from "@tanstack/react-query";
import type {
  TransportRequest,
  TroopEntry,
  TroopType,
  TroopsDto,
  ResourcesDto,
} from "../../api/types";
import { TROOP_LABELS } from "../../config/game";
import { TravelEta } from "../travel-time/TravelEta";
import { useTravelTime } from "../travel-time/useTravelTime";
import { useMyVillages, useGameConfig } from "../../api/hooks/useQueries";
import { NumberInput } from "../NumberInput";
import { Icon } from "../Icon";
import { RESOURCE_ICONS, TROOP_ICONS } from "../../lib/helpers";

const ICON_SIZE = 18;

const TROOP_FIELDS: { type: TroopType; key: keyof TroopsDto }[] = [
  { type: "Swordsman", key: "swordsmen" },
  { type: "Archer", key: "archers" },
  { type: "Dogs", key: "dogs" },
  { type: "Horsemen", key: "horsemen" },
  { type: "LlamaRiders", key: "llamaRiders" },
  { type: "Settler", key: "settlers" },
];

export function TransportPanel({
  villageId,
  troops,
  resources,
  mutation,
  onClose,
}: {
  villageId: string;
  troops: TroopsDto;
  resources: ResourcesDto;
  mutation: UseMutationResult<unknown, unknown, TransportRequest, unknown>;
  onClose: () => void;
}) {
  const { data: myVillages } = useMyVillages();
  const { data: gameConfig } = useGameConfig();
  const { getSpeed } = useTravelTime();

  const targets = myVillages?.filter((v) => v.id !== villageId) ?? [];

  const [targetVillageId, setTargetVillageId] = useState("");
  const [counts, setCounts] = useState<Partial<Record<TroopType, number>>>({});
  const [wood, setWood] = useState(0);
  const [clay, setClay] = useState(0);
  const [iron, setIron] = useState(0);
  const [beer, setBeer] = useState(0);

  const setCount = (type: TroopType, n: number) =>
    setCounts((c) => ({ ...c, [type]: n }));

  const selected = TROOP_FIELDS
    .map(({ type }) => ({ type, count: counts[type] ?? 0 }))
    .filter((t) => t.count > 0);

  const selectedVillage = targets.find((v) => v.id === targetVillageId);
  const carryCapacity = selected.reduce(
    (sum, t) => sum + t.count * (gameConfig?.troops[t.type]?.carryCapacity ?? 0),
    0,
  );
  const resourcesUsed = wood + clay + iron + beer;
  const maxFor = (have: number, key: "wood" | "clay" | "iron" | "beer") => {
    if (!carryCapacity) return 0;
    const others = resourcesUsed - { wood, clay, iron, beer }[key];
    const remaining = carryCapacity - others;
    return Math.min(Math.floor(have), Math.max(0, remaining));
  };
  const hasResources = wood > 0 || clay > 0 || iron > 0 || beer > 0;
  const hasTroops = selected.length > 0;

  const speed = hasTroops
    ? Math.min(...selected.map((t) => getSpeed(t.type)))
    : getSpeed("Swordsman");

  const handleSend = () => {
    if (!targetVillageId || (!hasTroops && !hasResources)) return;
    const troopEntries: TroopEntry[] = selected.map((t) => ({
      troopType: t.type,
      count: t.count,
    }));
    mutation.mutate(
      {
        targetVillageId,
        troops: troopEntries,
        resources: { wood, clay, iron, beer },
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
        <div className="grid grid-cols-3 gap-1">
          {TROOP_FIELDS.map(({ type, key }) => (
            <div key={type}>
              <NumberInput
                label={<><Icon src={TROOP_ICONS[type]} size={ICON_SIZE} /> {TROOP_LABELS[type] ?? type}</>}
                value={counts[type] ?? 0}
                onChange={(n) => setCount(type, n)}
                max={troops[key]}
              />
            </div>
          ))}
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
            <NumberInput label={<><Icon src={RESOURCE_ICONS.wood} size={ICON_SIZE} /> Wood</>} value={wood} onChange={setWood} max={maxFor(resources.wood, "wood")} />
          </div>
          <div className="flex-1">
            <NumberInput label={<><Icon src={RESOURCE_ICONS.clay} size={ICON_SIZE} /> Clay</>} value={clay} onChange={setClay} max={maxFor(resources.clay, "clay")} />
          </div>
          <div className="flex-1">
            <NumberInput label={<><Icon src={RESOURCE_ICONS.iron} size={ICON_SIZE} /> Iron</>} value={iron} onChange={setIron} max={maxFor(resources.iron, "iron")} />
          </div>
          <div className="flex-1">
            <NumberInput label={<><Icon src={RESOURCE_ICONS.beer} size={ICON_SIZE} /> Beer</>} value={beer} onChange={setBeer} max={maxFor(resources.beer, "beer")} />
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
    </section>
  );
}
