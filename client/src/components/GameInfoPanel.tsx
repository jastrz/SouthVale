import { useState } from "react";
import { useGameConfig } from "../api/hooks/useQueries";
import { BUILDING_ORDER, BUILDING_LABELS, BUILDING_DESCRIPTIONS, TROOP_LABELS } from "../config/game";
import { BUILDING_ICONS, TROOP_ICONS, RESOURCE_ICONS, UI_ICONS, parseTimeSpanMs, formatTime } from "../lib/helpers";
import { Icon } from "./Icon";
import { CollapsibleSection } from "./CollapsibleSection";
import type { BuildingLevelConfigDto, GameConfigDto } from "../api/types";

const HOW_TO_SECTIONS: {
  icon: string;
  title: string;
  body: React.ReactNode | ((config: GameConfigDto) => React.ReactNode);
}[] = [
  {
    icon: RESOURCE_ICONS.wood,
    title: "Resources & Upkeep",
    body: (
      <p>
        Four resources: <Icon src={RESOURCE_ICONS.wood} size={14} />{" "}
        <b>wood</b>, <Icon src={RESOURCE_ICONS.clay} size={14} /> <b>clay</b>,{" "}
        <Icon src={RESOURCE_ICONS.iron} size={14} /> <b>iron</b> and{" "}
        <Icon src={RESOURCE_ICONS.beer} size={14} /> <b>beer</b>.{" "}
        <Icon src={BUILDING_ICONS.WoodCutter} size={14} />{" "}
        <b>WoodCutter</b>, <Icon src={BUILDING_ICONS.ClayPit} size={14} />{" "}
        <b>ClayPit</b>, <Icon src={BUILDING_ICONS.IronMine} size={14} />{" "}
        <b>IronMine</b> and <Icon src={BUILDING_ICONS.Brewery} size={14} />{" "}
        <b>Brewery</b> produce them each hour. The{" "}
        <Icon src={BUILDING_ICONS.Warehouse} size={14} /> <b>Warehouse</b>{" "}
        sets your storage cap — production above it is lost. Every troop
        drinks <Icon src={RESOURCE_ICONS.beer} size={14} /> <b>beer</b> per
        hour; if your beer runs out, troops <b>run away</b>.
      </p>
    ),
  },
  {
    icon: BUILDING_ICONS.TownHall,
    title: "Buildings",
    body: (
      <p>
        The <Icon src={BUILDING_ICONS.TownHall} size={14} /> <b>TownHall</b>{" "}
        speeds up construction. <Icon src={BUILDING_ICONS.Barracks} size={14} />{" "}
        <b>Barracks</b> trains infantry,{" "}
        <Icon src={BUILDING_ICONS.Stable} size={14} /> <b>Stable</b> trains
        cavalry. <Icon src={BUILDING_ICONS.TradePost} size={14} />{" "}
        <b>TradePost</b> enables trading, the{" "}
        <Icon src={BUILDING_ICONS.Wall} size={14} /> <b>Wall</b> multiplies
        your defense, and the <Icon src={BUILDING_ICONS.Cranny} size={14} />{" "}
        <b>Cranny</b> hides resources from loot. Details and exact numbers per
        level are in the Buildings section below.
      </p>
    ),
  },
  {
    icon: BUILDING_ICONS.TradePost,
    title: "Expand & Trade",
    body: (config) => (
      <>
        <p>
          Found new villages: train a <Icon src={TROOP_ICONS.Settler} size={14} />{" "}
          <b>Settler</b> at the <Icon src={BUILDING_ICONS.Barracks} size={14} />{" "}
          <b>Barracks</b>, click an empty grass tile on the map and send them
          out — up to {config.maxVillagesPerPlayer} villages per player.{" "}
          <b>Settler</b> training cost doubles with each village.
        </p>
        <p>
          The{" "}
          <Icon src={BUILDING_ICONS.TradePost} size={14} /> <b>TradePost</b>{" "}
          converts resources into others (higher level = better rate), and{" "}
          <b>Transport</b> ships resources and troops between your villages in
          real time.
        </p>
      </>
    ),
  },
  {
    icon: TROOP_ICONS.Swordsman,
    title: "Troops & Attacks",
    body: (
      <p>
        Train troops at <Icon src={BUILDING_ICONS.Barracks} size={14} />{" "}
          <b>Barracks</b> (
          <Icon src={TROOP_ICONS.Swordsman} size={14} /> <b>Swordsman</b>,{" "}
          <Icon src={TROOP_ICONS.Archer} size={14} /> <b>Archer</b>,{" "}
          <Icon src={TROOP_ICONS.Dogs} size={14} /> <b>Dogs</b>) and{" "}
          <Icon src={BUILDING_ICONS.Stable} size={14} /> <b>Stable</b> (
          <Icon src={TROOP_ICONS.Horsemen} size={14} /> <b>Horsemen</b>,{" "}
          <Icon src={TROOP_ICONS.LlamaRiders} size={14} /> <b>LlamaRiders</b>).
          Select a village on the map and send your army to attack: attack power
          comes from troop stats plus <Icon src={BUILDING_ICONS.Barracks} size={14} /> <b>Barracks</b> / <Icon src={BUILDING_ICONS.Stable} size={14} /> <b>Stable</b> bonuses,
          defense from the target's <Icon src={BUILDING_ICONS.Wall} size={14} />{" "}
          <b>Wall</b>. Loot is limited by your carry capacity and the target's{" "}
          <Icon src={BUILDING_ICONS.Cranny} size={14} /> <b>Cranny</b>. Survivors
          return home with the loot. Travel takes real time — the group moves
          at the slowest troop's speed.
      </p>
    ),
  },

  {
    icon: UI_ICONS.notifications,
    title: "Reports",
    body: (
      <p>
        Every attack, <b>transport</b> and <b>settlement</b> produces a report
        with troops sent, lost and survived, plus loot — check the{" "}
        <Icon src={UI_ICONS.notifications} size={14} /> <b>Notifications</b>{" "}
        tab.
      </p>
    ),
  },
  {
    icon: UI_ICONS.actions,
    title: "Barbarians",
    body: (
      <p>
        Barbarian villages are NPC settlements that build, train troops and
        raid players within range. They grow stronger over time and defend
        their loot — but a well-prepared attack pays off. Yellow-colored village
        names on the map mark barbarians.
      </p>
    ),
  },
];

export function GameInfoPanel() {
  const { data: config, isLoading } = useGameConfig();
  const [openHowTo, setOpenHowTo] = useState(false);
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
            label={<span className="flex items-center gap-1.5"><Icon src={UI_ICONS.info} size={16} /> How to play</span>}
            open={openHowTo}
            onToggle={() => setOpenHowTo(!openHowTo)}
            className="rounded-lg bg-slate-800/85 px-3 py-1 text-slate-200 hover:text-white"
          >
            <div className="flex flex-col gap-2.5 text-xs text-slate-300">
              {HOW_TO_SECTIONS.map((s) => (
                <div key={s.title} className="rounded-lg bg-slate-800/85 px-3 py-2">
                  <p className="mb-0.5 flex items-center gap-1.5 font-bold text-slate-200">
                    <Icon src={s.icon} size={14} /> {s.title}
                  </p>
                  {typeof s.body === "function" ? s.body(config) : s.body}
                </div>
              ))}
            </div>
          </CollapsibleSection>

          <CollapsibleSection
            label={<span className="flex items-center gap-1.5"><Icon src={UI_ICONS.buildings} size={16} /> Buildings</span>}
            open={openBuildings}
            onToggle={() => setOpenBuildings(!openBuildings)}
            className="rounded-lg bg-slate-800/85 px-3 py-1 text-slate-200 hover:text-white"
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
                      className="ml-4 rounded-lg bg-slate-700/85 px-3 py-1 text-slate-300 hover:text-white"
                    >
                      <div className="px-3 pb-2">
                        <p className="mb-2 rounded-lg bg-slate-800/85 px-2 py-1 text-[11px] text-slate-300">
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
            className="rounded-lg bg-slate-800/85 px-3 py-1 text-slate-200 hover:text-white"
          >
            <div className="rounded-xl overflow-hidden overflow-x-auto">
              <table className="w-full text-left text-xs">
                <thead>
                  <tr className="text-slate-400 tracking-wider bg-slate-800/85">
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
                      className="border-t border-slate-700 bg-slate-800/85 text-white"
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
            className="rounded-lg bg-slate-800/85 px-3 py-1 text-slate-200 hover:text-white"
          >
            <div className="flex flex-col gap-1.5 rounded-lg bg-slate-800/85 px-3 py-2 text-xs text-slate-300">
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
          <tr className="text-slate-500 bg-slate-800/85">
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
              className="border-t border-slate-700 bg-slate-800/85 text-white"
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
