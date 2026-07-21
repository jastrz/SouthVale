import { useState } from "react";
import { TROOP_LABELS } from "../../config/game";
import { TROOP_ICONS } from "../../lib/helpers";
import { Icon } from "../Icon";

import type { UseMutationResult } from "@tanstack/react-query";

import type {
  TroopType,
  TrainRequest,
  TroopConfigDto,
  ResourcesDto,
  TroopsDto,
  BuildingDto,
} from "../../api/types";
import { useGameConfig } from "../../api/hooks/useQueries";
import { Tooltip } from "../Tooltip";
import { formatTime, parseTimeSpanMs } from "../../lib/helpers";
import { ResourceCost } from "./ResourceCost";
import { NumberInput } from "../NumberInput";

function TroopTooltip({ config }: { config: TroopConfigDto }) {
  return (
    <div className="space-y-1">
      <div className="font-semibold text-white">
        {TROOP_LABELS[config.type as TroopType] ?? config.type}
      </div>
      <div className="border-t border-slate-700" />
      <div className="grid grid-cols-[auto_1fr] gap-x-3">
        <div className="text-slate-300">Attack:</div>
        <div className="text-white">{config.attack}</div>

        <div className="text-slate-300">Defense:</div>
        <div className="text-white">{config.defense}</div>

        <div className="text-slate-300">Carry:</div>
        <div className="text-white">{config.carryCapacity}</div>

        <div className="text-slate-300">Speed:</div>
        <div className="text-white">{config.speed}</div>

        <div className="text-slate-300">Time:</div>
        <div className="text-white">
          {formatTime(parseTimeSpanMs(config.trainingTime))}
        </div>
      </div>
      <div className="border-t border-slate-700" />
      <div className="text-slate-300">Cost</div>
      <ResourceCost value={config.trainingCost} />
    </div>
  );
}

export function TroopsPanel({
  resources,
  troops,
  mutation,
  buildings,
}: {
  resources: ResourcesDto;
  troops: TroopsDto;
  mutation: UseMutationResult<unknown, unknown, TrainRequest, unknown>;
  buildings: readonly BuildingDto[];
}) {
  const { data: gameConfig } = useGameConfig();
  const [orders, setOrders] = useState<Record<string, number>>({});

  const availableTypes: [string, TroopConfigDto][] =
    (Object.entries(gameConfig?.troops ?? {}) as [string, TroopConfigDto][])
      .filter(([, cfg]) => buildings.some(b => b.type === cfg.trainedAt && b.level >= 1));

  const troopCount = (type: string) =>
    ({ Swordsman: troops.swordsmen, Archer: troops.archers, Settler: troops.settlers, Dogs: troops.dogs, Horsemen: troops.horsemen, LlamaRiders: troops.llamaRiders } as Record<string, number>)[type] ?? 0;

  const maxFor = (type: string): number => {
    const unitCost = gameConfig?.troops[type]?.trainingCost;
    if (!unitCost) return 0;
    const otherOrders = Object.entries(orders).filter(([t, c]) => t !== type && c > 0);
    const committed = otherOrders.reduce(
      (a, [t, c]) => {
        const cost = gameConfig?.troops[t]?.trainingCost;
        return cost ? { wood: a.wood + cost.wood * c, clay: a.clay + cost.clay * c, iron: a.iron + cost.iron * c, beer: a.beer + cost.beer * c } : a;
      },
      { wood: 0, clay: 0, iron: 0, beer: 0 },
    );
    const remaining = { wood: resources.wood - committed.wood, clay: resources.clay - committed.clay, iron: resources.iron - committed.iron, beer: resources.beer - committed.beer };
    return Math.min(
      unitCost.wood > 0 ? Math.floor(remaining.wood / unitCost.wood) : Infinity,
      unitCost.clay > 0 ? Math.floor(remaining.clay / unitCost.clay) : Infinity,
      unitCost.iron > 0 ? Math.floor(remaining.iron / unitCost.iron) : Infinity,
      unitCost.beer > 0 ? Math.floor(remaining.beer / unitCost.beer) : Infinity,
    );
  };

  return (
    <section className="px-4 py-3">
      <div className="grid grid-cols-3 gap-2 text-xs">
        {availableTypes.map(([type, cfg]) => (
          <div key={type} className="flex flex-col items-center gap-1 rounded bg-slate-800/40 px-2 py-1.5 text-center">
            <Tooltip content={<TroopTooltip config={cfg} />}>
              <div className="flex flex-col items-center gap-0.5">
                <div className="flex items-center gap-3">
                <Icon src={TROOP_ICONS[type] ?? ""} size={32} />
                  <span className="font-medium text-white">{troopCount(type)}</span>
                </div>
                <span className="text-[10px] text-slate-400">{TROOP_LABELS[type] ?? type}</span>
              </div>
            </Tooltip>
            <NumberInput
              value={orders[type] ?? 0}
              onChange={(v) => setOrders((prev) => ({ ...prev, [type]: v }))}
              max={maxFor(type)}
            />
          </div>
        ))}
      </div>
      {Object.values(orders).some(c => c > 0) && (
        <button
          type="button"
          onClick={() => {
            const entries = Object.entries(orders).filter(([, c]) => c > 0);
            mutation.mutate({
              orders: entries.map(([t, c]) => ({ troopType: t as TroopType, count: c })),
            }, { onSuccess: () => setOrders({}) });
          }}
          disabled={mutation.isPending}
          className="mt-3 w-full cursor-pointer rounded bg-green-700 px-3 py-1.5 text-xs font-medium text-white transition-colors hover:bg-green-600 disabled:cursor-not-allowed disabled:opacity-40"
        >
          {mutation.isPending ? "Training…" : "Train"}
        </button>
      )}
    </section>
  );
}
