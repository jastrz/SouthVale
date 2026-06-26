import { useEffect, useState } from "react";
import { useGameStateStore } from "../store/gameStateStore";
import type { MapVillage } from "../api/types";
import { tooltipBase } from "../styles/styles";
import { Icon } from "./Icon";
import { RESOURCE_ICONS } from "../lib/helpers";

export function VillageTooltip() {
  const hoveredVillage = useGameStateStore((s) => s.hoveredVillage);
  const [mousePos, setMousePos] = useState<{ x: number; y: number }>({
    x: 0,
    y: 0,
  });

  useEffect(() => {
    const onMouseMove = (e: MouseEvent) => {
      setMousePos({ x: e.clientX, y: e.clientY });
    };
    window.addEventListener("mousemove", onMouseMove);
    return () => window.removeEventListener("mousemove", onMouseMove);
  }, []);

  if (!hoveredVillage) return null;

  return (
    <div
      className={`pointer-events-none fixed z-50 ${tooltipBase}`}
      style={{ left: mousePos.x + 14, top: mousePos.y + 14 }}
    >
      <TooltipBody village={hoveredVillage} />
    </div>
  );
}

function TooltipBody({ village }: { village: MapVillage }) {
  return (
    <>
      <div className="font-semibold tracking-wide">{village.name}</div>
      <div className="mt-0.5 text-[11px] tracking-wider text-slate-400 uppercase">
        {village.kind === "own" ? "Your Village" : "Foreign Village"} ·{" "}
        {village.coordinates.x}, {village.coordinates.y}
      </div>

      {village.kind === "own" ? (
        <PlayerVillageInfoBody village={village} />
      ) : (
        <EnemyVillageInfoBody village={village} />
      )}
    </>
  );
}

type OwnVillage = Extract<MapVillage, { kind: "own" }>;
type EnemyVillage = Extract<MapVillage, { kind: "enemy" }>;

function PlayerVillageInfoBody({ village }: { village: OwnVillage }) {
  const { resources, troops } = village;
  return (
    <>
      <div className="mt-2 grid grid-cols-2 gap-x-3 gap-y-0.5 text-xs">
        <span className="flex items-center gap-1 text-slate-400">
          <Icon src={RESOURCE_ICONS.wood} size={14} /> Wood
        </span>
        <span className="text-right">{resources.wood}</span>
        <span className="flex items-center gap-1 text-slate-400">
          <Icon src={RESOURCE_ICONS.clay} size={14} /> Clay
        </span>
        <span className="text-right">{resources.clay}</span>
        <span className="flex items-center gap-1 text-slate-400">
          <Icon src={RESOURCE_ICONS.iron} size={14} /> Iron
        </span>
        <span className="text-right">{resources.iron}</span>
        <span className="flex items-center gap-1 text-slate-400">
          <Icon src={RESOURCE_ICONS.crop} size={14} /> Crop
        </span>
        <span className="text-right">{resources.crop}</span>
      </div>
      <div className="mt-2 border-t border-slate-700 pt-2 grid grid-cols-2 gap-x-3 gap-y-0.5 text-xs">
        <span className="text-slate-400">Swordsmen</span>
        <span className="text-right">{troops.swordsmen}</span>
        <span className="text-slate-400">Archers</span>
        <span className="text-right">{troops.archers}</span>
        <span className="text-slate-400">Settlers</span>
        <span className="text-right">{troops.settlers}</span>
      </div>
    </>
  );
}

function EnemyVillageInfoBody({ village }: { village: EnemyVillage }) {
  return (
    <div className="mt-2 grid grid-cols-2 gap-x-3 gap-y-0.5 text-xs">
      <span className="text-slate-400">Population</span>
      <span className="text-right">{village.population}</span>
      <span className="text-slate-400">Owner</span>
      <span className="truncate text-right text-slate-300">
        {village.playerName}
      </span>
    </div>
  );
}
