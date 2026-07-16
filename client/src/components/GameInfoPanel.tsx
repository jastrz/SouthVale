import { useState } from "react";
import { useGameConfig } from "../api/hooks/useQueries";
import { BUILDING_ORDER, BUILDING_LABELS, TROOP_LABELS } from "../config/game";
import { BUILDING_ICONS, TROOP_ICONS, RESOURCE_ICONS, parseTimeSpanMs, formatTime } from "../lib/helpers";
import { Icon } from "./Icon";
import { CollapsibleSection } from "./CollapsibleSection";
import type { BuildingLevelConfigDto } from "../api/types";

export function GameInfoPanel() {
  const { data: config, isLoading } = useGameConfig();
  const [openBuildings, setOpenBuildings] = useState(false);
  const [openTroops, setOpenTroops] = useState(false);

  return (
    <div className="flex flex-col gap-6 pb-8">
      {/*<h2 className="text-xs font-bold tracking-widest text-slate-400 uppercase text-center">
        Game Info
      </h2>*/}

      {isLoading && (
        <p className="text-center text-xs text-slate-500">Loading...</p>
      )}

      {config && (
        <>
          <CollapsibleSection
            label="Buildings"
            open={openBuildings}
            onToggle={() => setOpenBuildings(!openBuildings)}
          >
            <div className="flex flex-col gap-3">
              {BUILDING_ORDER.map((type) => {
                const levels = config.buildings[type];
                if (!levels) return null;
                return <BuildingTable key={type} type={type} levels={levels} />;
              })}
            </div>
          </CollapsibleSection>

          <CollapsibleSection
            label="Troops"
            open={openTroops}
            onToggle={() => setOpenTroops(!openTroops)}
          >
            <div className="rounded-xl overflow-hidden overflow-x-auto">
              <table className="w-full text-left text-xs">
                <thead>
                  <tr className="text-slate-400 uppercase tracking-wider bg-slate-800/80">
                    <th className="px-2 py-2">Troop</th>
                    <th className="px-2 py-2">Training Cost</th>
                    <th className="px-2 py-2">Time</th>
                    <th className="px-2 py-2">Attack</th>
                    <th className="px-2 py-2">Defense</th>
                    <th className="px-2 py-2">Carry</th>
                    <th className="px-2 py-2">Speed</th>
                  </tr>
                </thead>
                <tbody>
                  {Object.entries(config.troops).map(([type, troop]) => (
                    <tr
                      key={type}
                      className="border-t border-slate-700 bg-slate-800/80 text-white"
                    >
                      <td className="px-2 py-1.5">
                        <span className="flex items-center gap-1.5">
                          {TROOP_ICONS[type] && <Icon src={TROOP_ICONS[type]} size={16} />}
                          {TROOP_LABELS[type] ?? type}
                        </span>
                      </td>
                      <td className="px-2 py-1.5">
                        <CostIcons value={troop.trainingCost} />
                      </td>
                      <td className="px-2 py-1.5">
                        {formatTime(parseTimeSpanMs(troop.trainingTime))}
                      </td>
                      <td className="px-2 py-1.5">{troop.attack}</td>
                      <td className="px-2 py-1.5">{troop.defense}</td>
                      <td className="px-2 py-1.5">{troop.carryCapacity}</td>
                      <td className="px-2 py-1.5">{troop.speed}/h</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </CollapsibleSection>
        </>
      )}
    </div>
  );
}

function BuildingTable({
  type,
  levels,
}: {
  type: string;
  levels: BuildingLevelConfigDto[];
}) {
  return (
    <div className="rounded-xl overflow-hidden">
      <table className="w-full text-left text-xs">
        <thead>
          <tr className="text-slate-400 uppercase tracking-wider bg-slate-800/80">
            <th className="px-2 py-1.5" colSpan={4}>
              <span className="flex items-center gap-1.5 text-white">
                {BUILDING_ICONS[type] && <Icon src={BUILDING_ICONS[type]} size={16} />}
                {BUILDING_LABELS[type] ?? type}
              </span>
            </th>
          </tr>
          <tr className="text-slate-500 bg-slate-800/80">
            <th className="px-2 py-1">Lv</th>
            <th className="px-2 py-1">Upgrade Cost</th>
            <th className="px-2 py-1">Time</th>
            <th className="px-2 py-1">Benefits</th>
          </tr>
        </thead>
        <tbody>
          {levels.map((l) => (
            <tr
              key={l.level}
              className="border-t border-slate-700 bg-slate-800/80 text-white"
            >
              <td className="px-2 py-1.5 align-top">{l.level}</td>
              <td className="px-2 py-1.5 align-top">
                <CostIcons value={l.upgradeCost} />
              </td>
              <td className="px-2 py-1.5 align-top">
                {formatTime(parseTimeSpanMs(l.upgradeTime))}
              </td>
              <td className="px-2 py-1.5">
                <div className="flex flex-col gap-0.5">
                  {l.warehouseCapacity > 0 && (
                    <span>Warehouse: {l.warehouseCapacity}</span>
                  )}
                  {l.granaryCapacity > 0 && (
                    <span>Granary: {l.granaryCapacity}</span>
                  )}
                  {l.productionPerHour && (
                    <span className="flex items-center gap-1">
                      Production: <CostIcons value={l.productionPerHour} perHour />
                    </span>
                  )}
                  {l.trainingSpeedMultiplier > 1 && (
                    <span>Training: {l.trainingSpeedMultiplier}x</span>
                  )}
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

function CostIcons({
  value,
  perHour,
}: {
  value: { wood: number; clay: number; iron: number; crop: number };
  perHour?: boolean;
}) {
  const entries = Object.entries(RESOURCE_ICONS).filter(
    ([key]) => value[key as keyof typeof value] > 0,
  );
  return (
    <span className="flex flex-wrap items-center gap-1">
      {entries.map(([key, icon]) => (
        <span key={key} className="flex items-center gap-0.5">
          <Icon src={icon} size={12} />
          {value[key as keyof typeof value]}
          {perHour && "/h"}
        </span>
      ))}
    </span>
  );
}
