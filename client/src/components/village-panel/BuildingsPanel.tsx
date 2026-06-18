import type { UseMutationResult } from "@tanstack/react-query";
import { BUILDING_LABELS, BUILDING_DESCRIPTIONS } from "../../config/game";
import type { BuildingType, BuildRequest } from "../../api/types";
import { formatTime, timeRemaining } from "./helpers";
import { useTick } from "../../hooks/useTick";

function BuildingCard({
  building,
  disabled,
  isUpgrading,
  timeLabel,
  onUpgrade,
}: {
  building: { id: string; type: string; level: number };
  disabled: boolean;
  isUpgrading: boolean;
  timeLabel?: string;
  onUpgrade: () => void;
}) {
  return (
    <div className="flex items-center justify-between rounded bg-slate-800/40 px-2.5 py-1.5 text-xs">
      <div>
        <span className="font-medium text-white">
          {BUILDING_LABELS[building.type] ?? building.type}
        </span>
        <span className="ml-2 text-slate-400">Lv.{building.level}</span>
        <div className="text-[10px] text-slate-500">
          {BUILDING_DESCRIPTIONS[building.type] ?? ""}
        </div>
      </div>
      {isUpgrading ? (
        <span className="whitespace-nowrap text-[11px] text-yellow-400">
          {timeLabel ?? "…"}
        </span>
      ) : (
        <button
          type="button"
          onClick={onUpgrade}
          disabled={disabled}
          className="cursor-pointer rounded bg-blue-600 px-2.5 py-1 text-[11px] font-medium text-white transition-colors hover:bg-blue-500 disabled:cursor-not-allowed disabled:opacity-40 disabled:hover:bg-blue-600"
        >
          Upgrade
        </button>
      )}
    </div>
  );
}

export function BuildingsPanel({
  buildings,
  buildOrders,
  mutation,
}: {
  buildings: readonly { id: string; type: string; level: number }[];
  buildOrders: readonly { buildingType: string; targetLevel: number; completesAt: string }[];
  mutation: UseMutationResult<unknown, Error, BuildRequest, unknown>;
}) {
  useTick();
  const hasBuildOrder = buildOrders.length > 0;
  const activeBuildOrder = hasBuildOrder ? buildOrders[0] : null;

  return (
    <section className="border-b border-slate-800 px-4 py-3">
      <h3 className="mb-2 text-xs font-bold tracking-widest text-slate-400 uppercase">
        Buildings
      </h3>
      <div className="flex flex-col gap-1.5">
        {buildings.map((b) => (
          <BuildingCard
            key={b.id}
            building={b}
            disabled={hasBuildOrder && activeBuildOrder?.buildingType !== b.type}
            isUpgrading={activeBuildOrder?.buildingType === b.type}
            timeLabel={
              activeBuildOrder?.buildingType === b.type && activeBuildOrder
                ? formatTime(timeRemaining(activeBuildOrder.completesAt))
                : undefined
            }
            onUpgrade={() => mutation.mutate({ buildingType: b.type as BuildingType })}
          />
        ))}
      </div>
      {mutation.isError && (
        <p className="mt-2 text-xs text-red-400">
          {mutation.error instanceof Error ? mutation.error.message : "Upgrade failed"}
        </p>
      )}
    </section>
  );
}
