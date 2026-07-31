import type { UseMutationResult } from "@tanstack/react-query";
import { BUILDING_LABELS, BUILDING_DESCRIPTIONS } from "../../config/game";
import type {
  BuildingType,
  BuildRequest,
  BuildingLevelConfigDto,
  ResourcesDto,
} from "../../api/types";
import {
  formatTime,
  parseTimeSpanMs,
  RESOURCE_ICONS,
  BUILDING_ICONS,
} from "../../lib/helpers";
import { ResourceCost } from "./ResourceCost";
import { useGameConfig } from "../../api/hooks/useQueries";
import { Icon } from "../Icon";
import { Tooltip } from "../Tooltip";
import { BUILDING_ORDER } from "../../config/game";

function BuildingTooltip({
  building,
  config,
  nextConfig,
  buildSpeedMultiplier = 1,
  globalBuildSpeed = 1,
}: {
  building: { type: string; level: number };
  config: BuildingLevelConfigDto | undefined;
  nextConfig: BuildingLevelConfigDto | undefined;
  buildSpeedMultiplier?: number;
  globalBuildSpeed?: number;
}) {
  const currentPerHour = config?.productionPerHour;
  const nextPerHour = nextConfig?.productionPerHour;

  return (
    <div className="space-y-3">
      <div className="flex items-center gap-1.5 font-semibold text-white">
        {BUILDING_ICONS[building.type] && (
          <Icon src={BUILDING_ICONS[building.type]} size={18} />
        )}
        {BUILDING_LABELS[building.type] ?? building.type}
      </div>
      {config && <div className="text-slate-400">Level {building.level}</div>}
      {(config?.warehouseCapacity ?? 0) > 0 && (
        <div className="text-slate-300">Capacity: {config!.warehouseCapacity}</div>
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
          {currentPerHour.beer > 0 && (
            <span className="flex items-center gap-1 text-slate-300 whitespace-nowrap">
              <Icon src={RESOURCE_ICONS.beer} size={12} /> {currentPerHour.beer}/h
            </span>
          )}
        </div>
      )}
      {(config?.barracksTrainingSpeed ?? 0) > 1 && building.type === "Barracks" && (
        <div className="text-slate-300">
          Training Speed: {config!.barracksTrainingSpeed}x
        </div>
      )}
      {(config?.stableTrainingSpeed ?? 0) > 1 && building.type === "Stable" && (
        <div className="text-slate-300">
          Training Speed: {config!.stableTrainingSpeed}x
        </div>
      )}
      {(config?.barracksAttackMultiplier ?? 0) > 1 && building.type === "Barracks" && (
        <div className="text-slate-300">
          Infantry Attack: {config!.barracksAttackMultiplier}x (Empire-wide)
        </div>
      )}
      {(config?.stableAttackMultiplier ?? 0) > 1 && building.type === "Stable" && (
        <div className="text-slate-300">
          Cavalry Attack: {config!.stableAttackMultiplier}x (Empire-wide)
        </div>
      )}
      {(config?.defenseMultiplier ?? 0) > 1 && (
        <div className="text-slate-300">
          Defense: {config!.defenseMultiplier}x
        </div>
      )}
      {(config?.crannyCapacity ?? 0) > 0 && (
        <div className="text-slate-300">
          Hides: {config!.crannyCapacity} of each resource
        </div>
      )}
      {(config?.tradeRate ?? 1) < 1 && (
        <div className="text-slate-300">
          Trade Rate: {config!.tradeRate}x
        </div>
      )}
      {config != null && (config.buildSpeedMultiplier > 1 || building.type === "TownHall") && (
        <div className="text-slate-300">
          Build Speed: {config.buildSpeedMultiplier}x
        </div>
      )}
      {nextConfig && (
        <>
          <div className="border-t border-slate-700 pt-1" />
          <div className="font-medium text-cyan-400">
            {config ? `Next Level (${nextConfig.level})` : `Level ${nextConfig.level}`}
          </div>
          <div className="text-slate-300">Cost</div>
          <ResourceCost value={nextConfig.upgradeCost} />
          <div className="border-t border-slate-700 pt-1" />
          <div className="text-slate-300">
            Time: {formatTime(parseTimeSpanMs(nextConfig.upgradeTime) / (buildSpeedMultiplier * globalBuildSpeed))}{buildSpeedMultiplier > 1 && <span className="text-green-400"> ({buildSpeedMultiplier}x build speed)</span>}
          </div>
          <div className="border-t border-slate-700 pt-1" />
          {nextConfig.warehouseCapacity > 0 && (
            <div className="text-slate-300">
              Capacity: {nextConfig.warehouseCapacity}
              {config && (
                <span className="text-green-400">
                  {" "}
                  (+{nextConfig.warehouseCapacity - config.warehouseCapacity})
                </span>
              )}
            </div>
          )}
          {nextPerHour && (
            <div className="flex flex-wrap gap-x-2.5 gap-y-0.5">
              <span className="text-slate-300">Production</span>
              {nextPerHour.wood > 0 && (
                <span className="flex items-center gap-1 text-slate-300 whitespace-nowrap">
                  <Icon src={RESOURCE_ICONS.wood} size={12} /> {nextPerHour.wood}/h
                  {currentPerHour && nextPerHour.wood > currentPerHour.wood && (
                    <span className="text-green-400">
                      (+{nextPerHour.wood - currentPerHour.wood}/h)
                    </span>
                  )}
                </span>
              )}
              {nextPerHour.clay > 0 && (
                <span className="flex items-center gap-1 text-slate-300 whitespace-nowrap">
                  <Icon src={RESOURCE_ICONS.clay} size={12} /> {nextPerHour.clay}/h
                  {currentPerHour && nextPerHour.clay > currentPerHour.clay && (
                    <span className="text-green-400">
                      (+{nextPerHour.clay - currentPerHour.clay}/h)
                    </span>
                  )}
                </span>
              )}
              {nextPerHour.iron > 0 && (
                <span className="flex items-center gap-1 text-slate-300 whitespace-nowrap">
                  <Icon src={RESOURCE_ICONS.iron} size={12} /> {nextPerHour.iron}/h
                  {currentPerHour && nextPerHour.iron > currentPerHour.iron && (
                    <span className="text-green-400">
                      (+{nextPerHour.iron - currentPerHour.iron}/h)
                    </span>
                  )}
                </span>
              )}
              {nextPerHour.beer > 0 && (
                <span className="flex items-center gap-1 text-slate-300 whitespace-nowrap">
                  <Icon src={RESOURCE_ICONS.beer} size={12} /> {nextPerHour.beer}/h
                  {currentPerHour && nextPerHour.beer > currentPerHour.beer && (
                    <span className="text-green-400">
                      (+{nextPerHour.beer - currentPerHour.beer}/h)
                    </span>
                  )}
                </span>
              )}
            </div>
          )}
          {nextConfig.barracksTrainingSpeed > 1 && building.type === "Barracks" && (
            <div className="text-slate-300">
              Training Speed: {nextConfig.barracksTrainingSpeed}x
              {config && (
                <span className="text-green-400">
                  {" "}
                  (+
                  {(
                    (nextConfig.barracksTrainingSpeed -
                      config.barracksTrainingSpeed) *
                    100
                  ).toFixed(0)}
                  %)
                </span>
              )}
            </div>
          )}
          {nextConfig.stableTrainingSpeed > 1 && building.type === "Stable" && (
            <div className="text-slate-300">
              Training Speed: {nextConfig.stableTrainingSpeed}x
              {config && (
                <span className="text-green-400">
                  {" "}
                  (+
                  {(
                    (nextConfig.stableTrainingSpeed -
                      config.stableTrainingSpeed) *
                    100
                  ).toFixed(0)}
                  %)
                </span>
              )}
            </div>
          )}
          {nextConfig.barracksAttackMultiplier > 1 && building.type === "Barracks" && (
            <div className="text-slate-300">
              Infantry Attack: {nextConfig.barracksAttackMultiplier}x (Empire-wide)
              {config && (
                <span className="text-green-400">
                  {" "}
                  (+{((nextConfig.barracksAttackMultiplier - config.barracksAttackMultiplier) * 100).toFixed(0)}%)
                </span>
              )}
            </div>
          )}
          {nextConfig.stableAttackMultiplier > 1 && building.type === "Stable" && (
            <div className="text-slate-300">
              Cavalry Attack: {nextConfig.stableAttackMultiplier}x (Empire-wide)
              {config && (
                <span className="text-green-400">
                  {" "}
                  (+{((nextConfig.stableAttackMultiplier - config.stableAttackMultiplier) * 100).toFixed(0)}%)
                </span>
              )}
            </div>
          )}
          {nextConfig.defenseMultiplier > 1 && (
            <div className="text-slate-300">
              Defense: {nextConfig.defenseMultiplier}x
              {config && (
                <span className="text-green-400">
                  {" "}
                  (+
                  {(
                    (nextConfig.defenseMultiplier -
                      config.defenseMultiplier) *
                    100
                  ).toFixed(0)}
                  %)
                </span>
              )}
            </div>
          )}
          {nextConfig.crannyCapacity > 0 && (
            <div className="text-slate-300">
              Hides: {nextConfig.crannyCapacity}
              {config && (
                <span className="text-green-400">
                  {" "}
                  (+{nextConfig.crannyCapacity - config.crannyCapacity})
                </span>
              )}
            </div>
          )}
          {nextConfig.tradeRate < 1 && (
            <div className="text-slate-300">
              Trade Rate: {nextConfig.tradeRate}x
              {config && (
                <span className="text-green-400">
                  {" "}
                  (+{((nextConfig.tradeRate - config.tradeRate) * 100).toFixed(0)}%)
                </span>
              )}
            </div>
          )}
          {(nextConfig.buildSpeedMultiplier > 1 || building.type === "TownHall") && (
            <div className="text-slate-300">
              Build Speed: {nextConfig.buildSpeedMultiplier}x
              {config && (
                <span className="text-green-400">
                  {" "}
                  (+{((nextConfig.buildSpeedMultiplier - config.buildSpeedMultiplier) * 100).toFixed(0)}%)
                </span>
              )}
            </div>
          )}
        </>
      )}
    </div>
  );
}

function BuildingCard({
  building,
  orders,
  disabled,
  onUpgrade,
  maxLevel,
  resources,
  buildSpeedMultiplier = 1,
  globalBuildSpeed = 1,
}: {
  building: { id: string; type: string; level: number };
  orders: readonly { completesAt: string; targetLevel: number }[];
  disabled?: boolean;
  onUpgrade: () => void;
  maxLevel: number;
  resources?: ResourcesDto;
  buildSpeedMultiplier?: number;
  globalBuildSpeed?: number;
}) {
  const { data: config } = useGameConfig();
  const isNew = building.level === 0;
  const nextOrder =
    orders.length > 0
      ? orders.reduce((a, b) => (a.completesAt < b.completesAt ? a : b))
      : null;

  const levels = config?.buildings[building.type];
  const currentCfg = isNew ? undefined : levels?.find((l) => l.level === building.level);
  const highestQueued = orders.reduce(
    (max, o) => Math.max(max, o.targetLevel),
    building.level,
  );
  const nextCfg = levels?.find((l) => l.level === (isNew ? 1 : highestQueued + 1));

  const canAfford = !nextCfg || !resources || (
    resources.wood >= nextCfg.upgradeCost.wood &&
    resources.clay >= nextCfg.upgradeCost.clay &&
    resources.iron >= nextCfg.upgradeCost.iron &&
    resources.beer >= nextCfg.upgradeCost.beer
  );

  return (
    <Tooltip
      content={
        (currentCfg || nextCfg) && (
          <BuildingTooltip
            building={building}
            config={currentCfg}
            nextConfig={nextCfg}
            buildSpeedMultiplier={buildSpeedMultiplier}
            globalBuildSpeed={globalBuildSpeed}
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
            <span>{isNew ? "Not built" : `Lv. ${building.level}`}</span>
            <span className="text-[12px] italic text-slate-500">
              {BUILDING_DESCRIPTIONS[building.type] ?? ""}
            </span>
          </span>
          {nextOrder && (
              <div className="mt-0.5 flex items-center gap-2 text-slate-400">
                <span>→ {nextOrder.targetLevel}</span>
                {orders.length > 1 && (
                  <span className="text-[10px] text-slate-500">
                    +{orders.length - 1} more
                  </span>
                )}
              </div>
            )}
        </div>
        <button
          type="button"
          onClick={onUpgrade}
          disabled={disabled || building.level >= maxLevel || !canAfford}
          className="cursor-pointer rounded bg-blue-600 px-2.5 p-2 text-[11px] font-medium text-white transition-colors hover:bg-blue-500 disabled:cursor-not-allowed disabled:opacity-40 disabled:hover:bg-blue-600"
        >
          {disabled ? "..." : isNew ? "Build" : building.level >= maxLevel ? "Max" : "Upgrade"}
        </button>
      </div>
    </Tooltip>
  );
}

export function BuildingsPanel({
  buildings,
  buildOrders,
  mutation,
  resources,
}: {
  buildings: readonly { id: string; type: string; level: number }[];
  buildOrders: readonly {
    buildingType: string;
    targetLevel: number;
    completesAt: string;
  }[];
  mutation: UseMutationResult<unknown, unknown, BuildRequest, unknown>;
  resources?: ResourcesDto;
}) {
  const { data: gameConfig } = useGameConfig();

  const byType = Object.fromEntries(buildings.map((b) => [b.type, b]));
  const maxLevels = useMaxLevels();

  const townHall = buildings.find((b) => b.type === "TownHall");
  const buildSpeedMultiplier = townHall && gameConfig
    ? gameConfig.buildings["TownHall"]?.find((l) => l.level === townHall.level)?.buildSpeedMultiplier ?? 1
    : 1;

  const allBuildings = BUILDING_ORDER.map((type) =>
    byType[type] ?? { id: `new-${type}`, type, level: 0 },
  );

  return (
    <section className="px-4 py-3">
      {/*<h3 className="mb-2 text-xs font-bold tracking-widest text-slate-400 uppercase">
        Buildings
      </h3>*/}
      <div className="flex flex-col gap-1.5">
        {allBuildings.map((b) => (
          <BuildingCard
            key={b.id}
            building={b}
            orders={buildOrders.filter((o) => o.buildingType === b.type)}
            disabled={mutation.isPending}
            maxLevel={maxLevels[b.type] ?? 5}
            resources={resources}
            buildSpeedMultiplier={buildSpeedMultiplier}
            globalBuildSpeed={gameConfig?.buildSpeedMultiplier ?? 1}
            onUpgrade={() =>
              mutation.mutate({ buildingType: b.type as BuildingType })
            }
          />
        ))}
      </div>
    </section>
  );
}

function useMaxLevels(): Record<string, number> {
  const { data: config } = useGameConfig();
  if (!config) return {};
  return Object.fromEntries(
    Object.entries(config.buildings).map(([type, levels]) => [type, levels.length]),
  );
}
