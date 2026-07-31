import { useState } from "react";
import { useGameConfig } from "../api/hooks/useQueries";
import { BUILDING_ORDER, BUILDING_LABELS, BUILDING_DESCRIPTIONS, TROOP_LABELS } from "../config/game";
import { BUILDING_ICONS, TROOP_ICONS, RESOURCE_ICONS, UI_ICONS, parseTimeSpanMs, formatTime } from "../lib/helpers";
import { Icon } from "./Icon";
import { CollapsibleSection } from "./CollapsibleSection";
import type { BuildingLevelConfigDto } from "../api/types";

export function GameInfoPanel() {
  const { data: config, isLoading } = useGameConfig();
  const [openBuildings, setOpenBuildings] = useState(false);
  const [openTroops, setOpenTroops] = useState(false);
  const [openPlayers, setOpenPlayers] = useState(false);
  const [openBuildingTypes, setOpenBuildingTypes] = useState<Record<string, boolean>>({});

  const toggleBuildingType = (type: string) =>
    setOpenBuildingTypes((p) => ({ ...p, [type]: !p[type] }));

  return (
    <div className="flex flex-col gap-6 pb-8">
      {isLoading && (
        <p className="text-center text-xs text-slate-500">Loading...</p>
      )}

      {config && (
        <>
          <CollapsibleSection
            label={<span className="flex items-center gap-1.5"><Icon src={UI_ICONS.buildings} size={16} /> Buildings</span>}
            open={openBuildings}
            onToggle={() => setOpenBuildings(!openBuildings)}
            className="rounded-lg bg-slate-800/80 px-3 py-1 text-slate-200 hover:text-white"
          >
            <div className="flex flex-col gap-3">
              {BUILDING_ORDER.map((type) => {
                const levels = config.buildings[type];
                if (!levels) return null;
                return (
                  <div key={type}>
                    <CollapsibleSection
                      label={<span className="flex items-center gap-1.5">{BUILDING_ICONS[type] && <Icon src={BUILDING_ICONS[type]} size={16} />}{BUILDING_LABELS[type] ?? type}</span>}
                      open={openBuildingTypes[type] ?? false}
                      onToggle={() => toggleBuildingType(type)}
                      className="ml-4 rounded-lg bg-slate-700/80 px-3 py-1 text-slate-300 hover:text-white"
                    >
                      <div className="px-3 pb-2">
                        <p className="mb-2 rounded-lg bg-slate-800/80 px-2 py-1 text-[11px] text-slate-300">
                          {BUILDING_DESCRIPTIONS[type]}
                        </p>
                        <BuildingTable type={type} levels={levels} buildSpeedMultiplier={config.buildSpeedMultiplier} />
                      </div>
                    </CollapsibleSection>
                  </div>
                );
              })}
            </div>
          </CollapsibleSection>

          <CollapsibleSection
            label={<span className="flex items-center gap-1.5"><Icon src={UI_ICONS.troops} size={16} /> Troops</span>}
            open={openTroops}
            onToggle={() => setOpenTroops(!openTroops)}
            className="rounded-lg bg-slate-800/80 px-3 py-1 text-slate-200 hover:text-white"
          >
            <div className="rounded-xl overflow-hidden overflow-x-auto">
              <table className="w-full text-left text-xs">
                <thead>
                  <tr className="text-slate-400 tracking-wider bg-slate-800/80">
                    <th className="px-2 py-2">Troop</th>
                    <th className="px-2 py-2">Training Cost</th>
                    <th className="px-2 py-2">Time</th>
                    <th className="px-2 py-2">Attack</th>
                    <th className="px-2 py-2">Defense</th>
                    <th className="px-2 py-2">Carry</th>
                    <th className="px-2 py-2">Speed</th>
                    <th className="px-2 py-2"><span className="flex items-center gap-1"><Icon src={RESOURCE_ICONS.beer} size={10} /> Upkeep</span></th>
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
                  {formatTime(parseTimeSpanMs(troop.trainingTime) / (config.trainSpeedMultiplier || 1))}
                </td>
                      <td className="px-2 py-1.5">{troop.attack}</td>
                      <td className="px-2 py-1.5">{troop.defense}</td>
                      <td className="px-2 py-1.5">{troop.carryCapacity}</td>
                      <td className="px-2 py-1.5">{troop.speed}/h</td>
                      <td className="px-2 py-1.5"><span className="flex items-center gap-1"><Icon src={RESOURCE_ICONS.beer} size={10} /> {troop.upkeep}/h</span></td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </CollapsibleSection>

          <CollapsibleSection
            label={<span className="flex items-center gap-1.5"><Icon src={UI_ICONS.village} size={16} /> Players</span>}
            open={openPlayers}
            onToggle={() => setOpenPlayers(!openPlayers)}
            className="rounded-lg bg-slate-800/80 px-3 py-1 text-slate-200 hover:text-white"
          >
            <div className="flex flex-col gap-1.5 rounded-lg bg-slate-800/80 px-3 py-2 text-xs text-slate-300">
              <p>Village labels show the player name, village name and population.</p>
              <p>Player name color compares the player's leaderboard score to yours:</p>
              <div className="flex flex-col gap-0.5">
                <span className="rounded bg-[#ff4444]/15 px-1.5 py-0.5"><span style={{ color: "#ff4444" }}>■</span> at least 1.5x your score (stronger)</span>
                <span className="rounded bg-[#ffffff]/10 px-1.5 py-0.5"><span style={{ color: "#ffffff" }}>■</span> 0.5x - 1.5x your score (equal)</span>
                <span className="rounded bg-[#86e276]/15 px-1.5 py-0.5"><span style={{ color: "#86e276" }}>■</span> below 0.5x your score (weaker)</span>
                <span className="rounded bg-[#ffd700]/15 px-1.5 py-0.5"><span style={{ color: "#ffd700" }}>■</span> barbarian villages</span>
              </div>
              <p>Selected villages (your active village / enemy target) show a bobbing arrow above the village.</p>
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
  buildSpeedMultiplier,
}: {
  type: string;
  levels: BuildingLevelConfigDto[];
  buildSpeedMultiplier: number;
}) {
  return (
    <div className="rounded-xl overflow-hidden">
      <table className="w-full text-left text-xs">
        <thead>
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
                {formatTime(parseTimeSpanMs(l.upgradeTime) / (buildSpeedMultiplier || 1))}
              </td>
              <td className="px-2 py-1.5">
                <div className="flex flex-col gap-0.5">
                  {l.warehouseCapacity > 0 && (
                    <span>Warehouse: {l.warehouseCapacity}</span>
                  )}
                  {l.productionPerHour && (
                    <span className="flex items-center gap-1">
                      Production: <CostIcons value={l.productionPerHour} perHour />
                    </span>
                  )}
                  {l.barracksTrainingSpeed > 1 && type === "Barracks" && (
                    <span>Infantry Training: {l.barracksTrainingSpeed}x</span>
                  )}
                  {l.stableTrainingSpeed > 1 && type === "Stable" && (
                    <span>Cavalry Training: {l.stableTrainingSpeed}x</span>
                  )}
                  {l.barracksAttackMultiplier > 1 && type === "Barracks" && (
                    <span>Infantry Attack: {l.barracksAttackMultiplier}x (Empire-wide)</span>
                  )}
                  {l.stableAttackMultiplier > 1 && type === "Stable" && (
                    <span>Cavalry Attack: {l.stableAttackMultiplier}x (Empire-wide)</span>
                  )}
                  {l.defenseMultiplier > 1 && (
                    <span>Defense: {l.defenseMultiplier}x</span>
                  )}
                  {l.crannyCapacity > 0 && (
                    <span>Hidden: {l.crannyCapacity}</span>
                  )}
                  {l.tradeRate < 1 && (
                    <span>Trade: {l.tradeRate}x</span>
                  )}
                  {l.buildSpeedMultiplier > 1 && (
                    <span>Build Speed: {l.buildSpeedMultiplier}x</span>
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
  value: { wood: number; clay: number; iron: number; beer: number };
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
