import type { UseMutationResult } from "@tanstack/react-query";
import { BUILDING_LABELS, BUILDING_DESCRIPTIONS } from "../../config/game";
import type { BuildingType, BuildRequest } from "../../api/types";
import { formatTime, timeRemaining } from "./helpers";
import { useTick } from "../../hooks/useTick";
import { Icon } from "../Icon";

const BUILDING_ICONS: Record<string, string> = {
  WoodCutter: "/icons/buildings/woodcutter.png",
  ClayPit: "/icons/buildings/clay_pit.png",
  IronMine: "/icons/buildings/iron_mine.png",
  CropField: "/icons/buildings/crop_farm.png",
  Warehouse: "/icons/buildings/warehouse.png",
  Granary: "/icons/buildings/granary.png",
};

function BuildingCard({
  building,
  orders,
  disabled,
  onUpgrade,
}: {
  building: { id: string; type: string; level: number };
  orders: readonly { completesAt: string; targetLevel: number }[];
  disabled?: boolean;
  onUpgrade: () => void;
}) {
  const nextOrder =
    orders.length > 0
      ? orders.reduce((a, b) => (a.completesAt < b.completesAt ? a : b))
      : null;

  return (
    <div className="flex items-center justify-between rounded bg-slate-800/40 px-2.5 py-1.5 text-xs">
      <div>
        <span className="flex items-center gap-1.5 font-medium text-white">
          {BUILDING_ICONS[building.type] && (
            <Icon src={BUILDING_ICONS[building.type]} size={32} />
          )}
          {BUILDING_LABELS[building.type] ?? building.type}
        </span>
        <span className="flex items-center gap-4 text-slate-300">
          <span>Lv. {building.level}</span>
          <span className="text-[12px] italic text-slate-500">
            {BUILDING_DESCRIPTIONS[building.type] ?? ""}
          </span>
        </span>
        {nextOrder && (
          <>
            <span className="ml-1 text-slate-400">
              → {nextOrder.targetLevel}
            </span>
            <span className="ml-2 text-yellow-400">
              {formatTime(timeRemaining(nextOrder.completesAt))}
            </span>
            {orders.length > 1 && (
              <span className="ml-1 text-[10px] text-slate-500">
                +{orders.length - 1} more
              </span>
            )}
          </>
        )}
      </div>
      <button
        type="button"
        onClick={onUpgrade}
        disabled={disabled}
        className="cursor-pointer rounded bg-blue-600 px-2.5 p-2 text-[11px] font-medium text-white transition-colors hover:bg-blue-500 disabled:cursor-not-allowed disabled:opacity-40 disabled:hover:bg-blue-600"
      >
        {disabled ? "..." : "Upgrade"}
      </button>
    </div>
  );
}

export function BuildingsPanel({
  buildings,
  buildOrders,
  mutation,
}: {
  buildings: readonly { id: string; type: string; level: number }[];
  buildOrders: readonly {
    buildingType: string;
    targetLevel: number;
    completesAt: string;
  }[];
  mutation: UseMutationResult<unknown, Error, BuildRequest, unknown>;
}) {
  useTick();

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
            orders={buildOrders.filter((o) => o.buildingType === b.type)}
            disabled={mutation.isPending}
            onUpgrade={() =>
              mutation.mutate({ buildingType: b.type as BuildingType })
            }
          />
        ))}
      </div>
      {mutation.isError && (
        <p className="mt-2 text-xs text-red-400">
          {mutation.error instanceof Error
            ? mutation.error.message
            : "Upgrade failed"}
        </p>
      )}
    </section>
  );
}
