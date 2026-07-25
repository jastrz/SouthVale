import { useState } from "react";
import { TROOP_LABELS } from "../../config/game";
import { TROOP_ICONS, RESOURCE_ICONS } from "../../lib/helpers";
import { Icon } from "../Icon";

import type { UseMutationResult } from "@tanstack/react-query";

import type {
  TroopType,
  TrainRequest,
  TroopConfigDto,
  ResourcesDto,
  TroopsDto,
  BuildingDto
} from "../../api/types";
import { useGameConfig } from "../../api/hooks/useQueries";
import { Tooltip } from "../Tooltip";
import { formatTime, parseTimeSpanMs } from "../../lib/helpers";
import { ResourceCost } from "./ResourceCost";
import { NumberInput } from "../NumberInput";

function TroopTooltip({ config, cost }: { config: TroopConfigDto; cost: ResourcesDto }) {
  return (
    <div className="space-y-1">
      <div className="font-semibold text-white">
        {TROOP_LABELS[config.type as TroopType] ?? config.type}
      </div>
      <div className="text-[10px] leading-none"
        style={{ color: config.trainedAt === "Stable" ? "#f59e0b" : "#22d3ee" }}>
        {config.trainedAt === "Stable" ? "cavalry" : "infantry"}
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

        <div className="text-slate-300">Upkeep:</div>
        <div className="text-white"><span className="inline-flex items-center gap-0.5">{config.upkeep}/h<Icon src={RESOURCE_ICONS.beer} size={10} /></span></div>

        <div className="text-slate-300">Time:</div>
        <div className="text-white">
          {formatTime(parseTimeSpanMs(config.trainingTime))}
        </div>
      </div>
      <div className="border-t border-slate-700" />
      <div className="text-slate-300">Cost</div>
      <ResourceCost value={cost} />
      {config.type === "Settler" && (
        <div className="text-[10px] text-yellow-500">
          ×{cost.wood / config.trainingCost.wood} cost (villages + existing settlers)
        </div>
      )}
    </div>
  );
}

const settlerCostMultiplier = (villageCount: number, existingSettlers: number) =>
  Math.max(1, Math.pow(2, villageCount + existingSettlers - 1));

export function TroopsPanel({
  resources,
  troops,
  mutation,
  buildings,
  villageCount,
  settlersInTraining,
  settlersInMovement,
}: {
  resources: ResourcesDto;
  troops: TroopsDto;
  mutation: UseMutationResult<unknown, unknown, TrainRequest, unknown>;
  buildings: readonly BuildingDto[];
  villageCount: number;
  settlersInTraining: number;
  settlersInMovement?: number;
}) {
  const { data: gameConfig } = useGameConfig();
  const [orders, setOrders] = useState<Record<string, number>>({});

  const availableTypes: [string, TroopConfigDto][] =
    (Object.entries(gameConfig?.troops ?? {}) as [string, TroopConfigDto][])
      .filter(([, cfg]) => buildings.some(b => b.type === cfg.trainedAt && b.level >= 1));

  const troopCount = (type: string) =>
    ({ Swordsman: troops.swordsmen, Archer: troops.archers, Settler: troops.settlers, Dogs: troops.dogs, Horsemen: troops.horsemen, LlamaRiders: troops.llamaRiders } as Record<string, number>)[type] ?? 0;

  const effectiveCost = (type: string): ResourcesDto => {
    const base = gameConfig?.troops[type]?.trainingCost;
    if (!base) return { wood: 0, clay: 0, iron: 0, beer: 0 };
    if (type === "Settler") {
      const m = settlerCostMultiplier(villageCount, troops.settlers + settlersInTraining + (settlersInMovement ?? 0) + (orders[type] ?? 0));
      return { wood: base.wood * m, clay: base.clay * m, iron: base.iron * m, beer: base.beer * m };
    }
    return base;
  };

  const totalBatchCost = (): ResourcesDto => {
    const total = { wood: 0, clay: 0, iron: 0, beer: 0 };
    let settlerCount = 0;
    for (const [type, count] of Object.entries(orders)) {
      if (count <= 0) continue;
      const baseCost = gameConfig?.troops[type]?.trainingCost;
      if (!baseCost) continue;
      if (type === "Settler") {
        const k = villageCount + troops.settlers + settlersInTraining + (settlersInMovement ?? 0) + settlerCount - 1;
        const m = Math.max(1, Math.pow(2, k) * (Math.pow(2, count) - 1));
        total.wood += baseCost.wood * m;
        total.clay += baseCost.clay * m;
        total.iron += baseCost.iron * m;
        total.beer += baseCost.beer * m;
        settlerCount += count;
      } else {
        total.wood += baseCost.wood * count;
        total.clay += baseCost.clay * count;
        total.iron += baseCost.iron * count;
        total.beer += baseCost.beer * count;
      }
    }
    return total;
  };

  const maxFor = (type: string): number => {
    const baseCost = gameConfig?.troops[type]?.trainingCost;
    if (!baseCost) return 0;
    const otherOrders = Object.entries(orders).filter(([t, c]) => t !== type && c > 0);
    const committed = otherOrders.reduce(
      (a, [t, c]) => {
        const cost = effectiveCost(t);
        return { wood: a.wood + cost.wood * c, clay: a.clay + cost.clay * c, iron: a.iron + cost.iron * c, beer: a.beer + cost.beer * c };
      },
      { wood: 0, clay: 0, iron: 0, beer: 0 },
    );
    const remaining = { wood: resources.wood - committed.wood, clay: resources.clay - committed.clay, iron: resources.iron - committed.iron, beer: resources.beer - committed.beer };

    if (type === "Settler") {
      let count = 0;
      let wood = remaining.wood, clay = remaining.clay, iron = remaining.iron, beer = remaining.beer;
      const existing = troops.settlers + settlersInTraining + (settlersInMovement ?? 0);
      while (true) {
        const m = settlerCostMultiplier(villageCount, existing + count);
        const nextCost = { wood: baseCost.wood * m, clay: baseCost.clay * m, iron: baseCost.iron * m, beer: baseCost.beer * m };
        if (wood < nextCost.wood || clay < nextCost.clay || iron < nextCost.iron || beer < nextCost.beer) break;
        wood -= nextCost.wood; clay -= nextCost.clay; iron -= nextCost.iron; beer -= nextCost.beer;
        count++;
      }
      return count;
    }

    return Math.min(
      baseCost.wood > 0 ? Math.floor(remaining.wood / baseCost.wood) : Infinity,
      baseCost.clay > 0 ? Math.floor(remaining.clay / baseCost.clay) : Infinity,
      baseCost.iron > 0 ? Math.floor(remaining.iron / baseCost.iron) : Infinity,
      baseCost.beer > 0 ? Math.floor(remaining.beer / baseCost.beer) : Infinity,
    );
  };

  return (
    <section className="px-4 py-3">
      <div className="grid grid-cols-3 gap-2 text-xs">
        {availableTypes.map(([type, cfg]) => (
          <div key={type} className="flex flex-col items-center gap-1 rounded bg-slate-800/40 px-2 py-1.5 text-center">
            <Tooltip content={<TroopTooltip config={cfg} cost={effectiveCost(type)} />}>
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
        <>
          <div className="mt-2 border-t border-slate-700 pt-2">
            <div className="mb-1 text-sm text-slate-300">Total cost:</div>
            <ResourceCost value={totalBatchCost()} />
          </div>
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
        </>
      )}
    </section>
  );
}
