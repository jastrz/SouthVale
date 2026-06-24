import type { UseMutationResult } from "@tanstack/react-query";
import { BUILDING_LABELS, BUILDING_DESCRIPTIONS } from "../../config/game";
import type { BuildingType, BuildRequest, BuildingLevelConfigDto } from "../../api/types";
import { formatTime, timeRemaining, parseTimeSpanMs, RESOURCE_ICONS } from "./helpers";
import { ResourceCost } from "./ResourceCost";
import { useTick } from "../../hooks/useTick";
import { useGameConfig } from "../../api/hooks/useQueries";
import { Icon } from "../Icon";
import { Tooltip } from "../Tooltip";

function BuildingTooltip({
  building,
  config,
  nextConfig,
}: {
  building: { type: string; level: number };
  config: BuildingLevelConfigDto;
  nextConfig: BuildingLevelConfigDto | undefined;
}) {
  const currentPerHour = config.productionPerHour;
  const nextPerHour = nextConfig?.productionPerHour;

  return (
    <div className="space-y-3">
      <div className="flex items-center gap-1.5 font-semibold text-white">
        {BUILDING_ICONS[building.type] && <Icon src={BUILDING_ICONS[building.type]} size={18} />}
        {BUILDING_LABELS[building.type] ?? building.type}
      </div>
      <div className="text-slate-400">Level {building.level}</div>

      {config.warehouseCapacity > 0 && (
        <div className="text-slate-300">Capacity: {config.warehouseCapacity}</div>
      )}
      {config.granaryCapacity > 0 && (
        <div className="text-slate-300">Capacity: {config.granaryCapacity}</div>
      )}
      {currentPerHour && (
        <div className="flex flex-wrap gap-x-2.5 gap-y-0.5">
          <span className="text-slate-300">Production</span>
          {currentPerHour.wood > 0 && (
            <span className="flex items-center gap-1 text-slate-300 whitespace-nowrap">
              <Icon src={RESOURCE_ICONS.wood} size={12} /> {currentPerHour.wood}/h
            </span>
          )}
          {currentPerHour.clay > 0 && (
            <span className="flex items-center gap-1 text-slate-300 whitespace-nowrap">
              <Icon src={RESOURCE_ICONS.clay} size={12} /> {currentPerHour.clay}/h
            </span>
          )}
          {currentPerHour.iron > 0 && (
            <span className="flex items-center gap-1 text-slate-300 whitespace-nowrap">
              <Icon src={RESOURCE_ICONS.iron} size={12} /> {currentPerHour.iron}/h
            </span>
          )}
          {currentPerHour.crop > 0 && (
            <span className="flex items-center gap-1 text-slate-300 whitespace-nowrap">
              <Icon src={RESOURCE_ICONS.crop} size={12} /> {currentPerHour.crop}/h
            </span>
          )}
        </div>
      )}
      {config.trainingSpeedMultiplier > 1 && (
        <div className="text-slate-300">Training Speed: {config.trainingSpeedMultiplier}x</div>
      )}

      {nextConfig && (
        <>
          <div className="border-t border-slate-700 pt-1" />
          <div className="font-medium text-cyan-400">Next Level ({nextConfig.level})</div>
          <div className="text-slate-300">Cost</div>
          <ResourceCost value={nextConfig.upgradeCost} />
          <div className="border-t border-slate-700 pt-1" />
          <div className="text-slate-300">Time: {formatTime(parseTimeSpanMs(nextConfig.upgradeTime))}</div>
          <div className="border-t border-slate-700 pt-1" />
          {nextConfig.warehouseCapacity > 0 && (
            <div className="text-slate-300">
              Capacity: {nextConfig.warehouseCapacity}
              <span className="text-green-400"> (+{nextConfig.warehouseCapacity - config.warehouseCapacity})</span>
            </div>
          )}
          {nextConfig.granaryCapacity > 0 && (
            <div className="text-slate-300">
              Capacity: {nextConfig.granaryCapacity}
              <span className="text-green-400"> (+{nextConfig.granaryCapacity - config.granaryCapacity})</span>
            </div>
          )}
          {nextPerHour && (
            <div className="flex flex-wrap gap-x-2.5 gap-y-0.5">
              <span className="text-slate-300">Production</span>
              {nextPerHour.wood > 0 && (
                <span className="flex items-center gap-1 text-slate-300 whitespace-nowrap">
                  <Icon src={RESOURCE_ICONS.wood} size={12} /> {nextPerHour.wood}/h
                  {currentPerHour && nextPerHour.wood > currentPerHour.wood && (
                    <span className="text-green-400">(+{nextPerHour.wood - currentPerHour.wood}/h)</span>
                  )}
                </span>
              )}
              {nextPerHour.clay > 0 && (
                <span className="flex items-center gap-1 text-slate-300 whitespace-nowrap">
                  <Icon src={RESOURCE_ICONS.clay} size={12} /> {nextPerHour.clay}/h
                  {currentPerHour && nextPerHour.clay > currentPerHour.clay && (
                    <span className="text-green-400">(+{nextPerHour.clay - currentPerHour.clay}/h)</span>
                  )}
                </span>
              )}
              {nextPerHour.iron > 0 && (
                <span className="flex items-center gap-1 text-slate-300 whitespace-nowrap">
                  <Icon src={RESOURCE_ICONS.iron} size={12} /> {nextPerHour.iron}/h
                  {currentPerHour && nextPerHour.iron > currentPerHour.iron && (
                    <span className="text-green-400">(+{nextPerHour.iron - currentPerHour.iron}/h)</span>
                  )}
                </span>
              )}
              {nextPerHour.crop > 0 && (
                <span className="flex items-center gap-1 text-slate-300 whitespace-nowrap">
                  <Icon src={RESOURCE_ICONS.crop} size={12} /> {nextPerHour.crop}/h
                  {currentPerHour && nextPerHour.crop > currentPerHour.crop && (
                    <span className="text-green-400">(+{nextPerHour.crop - currentPerHour.crop}/h)</span>
                  )}
                </span>
              )}
            </div>
          )}
          {nextConfig.trainingSpeedMultiplier > 1 && (
            <div className="text-slate-300">
              Speed: {nextConfig.trainingSpeedMultiplier}x
              <span className="text-green-400"> (+{((nextConfig.trainingSpeedMultiplier - config.trainingSpeedMultiplier) * 100).toFixed(0)}%)</span>
            </div>
          )}
        </>
      )}
    </div>
  );
}

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
  const { data: config } = useGameConfig();
  const nextOrder =
    orders.length > 0
      ? orders.reduce((a, b) => (a.completesAt < b.completesAt ? a : b))
      : null;

  const levels = config?.buildings[building.type];
  const currentCfg = levels?.find((l) => l.level === building.level);
  const nextCfg = levels?.find((l) => l.level === building.level + 1);

  return (
    <Tooltip
      content={
        currentCfg && (
          <BuildingTooltip
            building={building}
            config={currentCfg}
            nextConfig={nextCfg}
          />
        )
      }
    >
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
    </Tooltip>
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
