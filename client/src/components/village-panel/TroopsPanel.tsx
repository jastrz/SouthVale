import { useState } from "react";
import type { UseMutationResult } from "@tanstack/react-query";
import { TROOP_LABELS } from "../../config/game";
import type { TroopType, TrainRequest, TroopConfigDto } from "../../api/types";
import { useGameConfig } from "../../api/hooks/useQueries";
import { Tooltip } from "../Tooltip";
import { formatTime, parseTimeSpanMs } from "../../lib/helpers";
import { ResourceCost } from "./ResourceCost";

function TroopTooltip({ config }: { config: TroopConfigDto }) {
  return (
    <div className="space-y-3">
      <div className="font-semibold text-white">
        {TROOP_LABELS[config.type as TroopType] ?? config.type}
      </div>
      <div className="flex gap-3 text-slate-300">
        <span>ATK: {config.attack}</span>
        <span>DEF: {config.defense}</span>
      </div>
      <div className="flex gap-3 text-slate-300">
        <span>Carry: {config.carryCapacity}</span>
        <span>Speed: {config.speed}</span>
        <span>Upkeep: {config.upkeep}</span>
      </div>
      <div className="text-slate-300">
        Time: {formatTime(parseTimeSpanMs(config.trainingTime))}
      </div>
      <div className="border-t border-slate-700 pt-1" />
      <div className="text-slate-300">Cost</div>
      <ResourceCost value={config.trainingCost} />
    </div>
  );
}

function TroopCount({ label, count }: { label: string; count: number }) {
  return (
    <div className="rounded bg-slate-800/40 px-2 py-1.5 text-center">
      <div className="font-medium text-white">{count}</div>
      <div className="text-[10px] text-slate-400">{label}</div>
    </div>
  );
}

function TrainingForm({
  mutation,
}: {
  mutation: UseMutationResult<unknown, Error, TrainRequest, unknown>;
}) {
  const { data: gameConfig } = useGameConfig();
  const [orders, setOrders] = useState<Record<string, number>>({});

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
              <input
                type="number"
                min={0}
                value={orders[type] ?? ""}
                onChange={(e) =>
                  setOrders((prev) => ({
                    ...prev,
                    [type]: Math.max(0, Number.parseInt(e.target.value) || 0),
                  }))
                }
                className="w-full rounded border border-slate-600 bg-slate-900 px-1.5 py-1 text-xs text-white outline-none focus:border-blue-500"
                placeholder="0"
              />
            </div>
          );
        })}
      </div>
      <button
        type="button"
        onClick={handleTrain}
        disabled={!hasOrders || mutation.isPending}
        className="w-full cursor-pointer rounded bg-green-700 px-3 py-1.5 text-xs font-medium text-white transition-colors hover:bg-green-600 disabled:cursor-not-allowed disabled:opacity-40 disabled:hover:bg-green-700"
      >
        {mutation.isPending ? "Training…" : "Train"}
      </button>
      {mutation.isError && (
        <p className="mt-1 text-xs text-red-400">
          {mutation.error instanceof Error
            ? mutation.error.message
            : "Training failed"}
        </p>
      )}
    </div>
  );
}

export function TroopsPanel({
  swordsmen,
  archers,
  settlers,
  mutation,
}: {
  swordsmen: number;
  archers: number;
  settlers: number;
  mutation: UseMutationResult<unknown, Error, TrainRequest, unknown>;
}) {
  return (
    <section className="border-b border-slate-800 px-4 py-3">
      <h3 className="mb-2 text-xs font-bold tracking-widest text-slate-400 uppercase">
        Troops
      </h3>
      <div className="mb-2 grid grid-cols-3 gap-1 text-xs">
        <TroopCount label="Swordsmen" count={swordsmen} />
        <TroopCount label="Archers" count={archers} />
        <TroopCount label="Settlers" count={settlers} />
      </div>
      <TrainingForm mutation={mutation} />
    </section>
  );
}
