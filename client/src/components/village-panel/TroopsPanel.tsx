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

function TroopCount({
  // label,
  count,
  icon,
}: {
  label: string;
  count: number;
  icon?: string;
}) {
  return (
    <div className="rounded bg-slate-800/40 px-2 py-1.5 text-center">
      {icon && (
        <div className="mb-1 flex justify-center">
          <Icon src={icon} size={32} />
        </div>
      )}
      <div className="font-medium text-white">{count}</div>
      {/*<div className="text-[10px] text-slate-400">{label}</div>*/}
    </div>
  );
}

function TrainingForm({
  resources,
  mutation,
}: {
  resources: ResourcesDto;
  mutation: UseMutationResult<unknown, unknown, TrainRequest, unknown>;
}) {
  const { data: gameConfig } = useGameConfig();
  const [orders, setOrders] = useState<Record<string, number>>({});

  const orderCost = (type: string, count: number): ResourcesDto => {
    const c = gameConfig?.troops[type]?.trainingCost;
    return c
      ? {
          wood: c.wood * count,
          clay: c.clay * count,
          iron: c.iron * count,
          beer: c.beer * count,
        }
      : { wood: 0, clay: 0, iron: 0, beer: 0 };
  };

  const sumCost = (costs: ResourcesDto[]): ResourcesDto =>
    costs.reduce(
      (a, b) => ({
        wood: a.wood + b.wood,
        clay: a.clay + b.clay,
        iron: a.iron + b.iron,
        beer: a.beer + b.beer,
      }),
      { wood: 0, clay: 0, iron: 0, beer: 0 },
    );

  const maxFor = (type: string): number => {
    const unitCost = gameConfig?.troops[type]?.trainingCost;
    if (!unitCost) return 0;

    // subtract resources already committed to other pending orders
    const otherOrders = Object.entries(orders).filter(
      ([t, c]) => t !== type && c > 0,
    );
    const committed = sumCost(otherOrders.map(([t, c]) => orderCost(t, c)));
    const remaining: ResourcesDto = {
      wood: resources.wood - committed.wood,
      clay: resources.clay - committed.clay,
      iron: resources.iron - committed.iron,
      beer: resources.beer - committed.beer,
    };

    return Math.min(
      unitCost.wood > 0 ? Math.floor(remaining.wood / unitCost.wood) : Infinity,
      unitCost.clay > 0 ? Math.floor(remaining.clay / unitCost.clay) : Infinity,
      unitCost.iron > 0 ? Math.floor(remaining.iron / unitCost.iron) : Infinity,
      unitCost.beer > 0 ? Math.floor(remaining.beer / unitCost.beer) : Infinity,
    );
  };

  const handleTrain = () => {
    const entries = Object.entries(orders).filter(([, count]) => count > 0);
    if (entries.length === 0) return;
    mutation.mutate(
      {
        orders: entries.map(([troopType, count]) => ({
          troopType: troopType as TroopType,
          count,
        })),
      },
      { onSuccess: () => setOrders({}) },
    );
  };

  const hasOrders = Object.values(orders).some((c) => c > 0);

  const totalCost = hasOrders
    ? sumCost(
        Object.entries(orders)
          .filter(([, c]) => c > 0)
          .map(([t, c]) => orderCost(t, c)),
      )
    : null;

  return (
    <div className="flex flex-col gap-3">
      <div className="flex gap-1">
        {(["Swordsman", "Archer", "Settler"] as const).map((type) => {
          const troopCfg = gameConfig?.troops[type];
          const label = (
            <label className="block text-[10px] text-slate-400">
              {TROOP_LABELS[type]}
            </label>
          );
          return (
            <div key={type} className="flex-1">
              {troopCfg ? (
                <Tooltip content={<TroopTooltip config={troopCfg} />}>
                  {label}
                </Tooltip>
              ) : (
                label
              )}
              <NumberInput
                value={orders[type] ?? 0}
                onChange={(v) => setOrders((prev) => ({ ...prev, [type]: v }))}
                max={maxFor(type)}
              />
            </div>
          );
        })}
      </div>
      {totalCost && <ResourceCost value={totalCost} />}
      <button
        type="button"
        onClick={handleTrain}
        disabled={!hasOrders || mutation.isPending}
        className="w-full cursor-pointer rounded bg-green-700 px-3 py-1.5 text-xs font-medium text-white transition-colors hover:bg-green-600 disabled:cursor-not-allowed disabled:opacity-40 disabled:hover:bg-green-700"
      >
        {mutation.isPending ? "Training…" : "Train"}
      </button>
    </div>
  );
}

export function TroopsPanel({
  resources,
  swordsmen,
  archers,
  settlers,
  mutation,
}: {
  resources: ResourcesDto;
  swordsmen: number;
  archers: number;
  settlers: number;
  mutation: UseMutationResult<unknown, unknown, TrainRequest, unknown>;
}) {
  const { data: gameConfig } = useGameConfig();
  const troopList: [string, TroopType, number, string][] = [
    ["Swordsmen", "Swordsman", swordsmen, TROOP_ICONS.Swordsman],
    ["Archers", "Archer", archers, TROOP_ICONS.Archer],
    ["Settlers", "Settler", settlers, TROOP_ICONS.Settler],
  ];

  return (
    <section className="px-4 py-3">
      {/*<h3 className="mb-2 text-xs font-bold tracking-widest text-slate-400 uppercase">
        Troops
      </h3>*/}
      <div className="mb-2 grid grid-cols-3 gap-1 text-xs">
        {troopList.map(([label, type, count, icon]) => {
          const troopCfg = gameConfig?.troops[type];
          return (
            <Tooltip
              key={type}
              content={troopCfg ? <TroopTooltip config={troopCfg} /> : null}
            >
              <TroopCount label={label} count={count} icon={icon} />
            </Tooltip>
          );
        })}
      </div>
      <TrainingForm resources={resources} mutation={mutation} />
    </section>
  );
}
