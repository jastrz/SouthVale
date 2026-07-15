import { useState } from "react";
import { useGameStateStore } from "../store/gameStateStore";
import { MapCanvas } from "../components/MapCanvas";
import { NotificationsPanel } from "../components/NotificationsPanel";
import { LeaderboardPanel } from "../components/LeaderboardPanel";
import { OverviewPanel } from "../components/overview-panel/OverviewPanel";
import { VillagePanel } from "../components/village-panel";
import { VillageTooltip } from "../components/VillageTooltip";
import { TopBar } from "../components/TopBar";

function Panel({
  side,
  open,
  onClose,
  children,
}: {
  side: "left" | "right";
  open: boolean;
  onClose: () => void;
  children: React.ReactNode;
}) {
  const translate = side === "left"
    ? (open ? "translate-x-0" : "-translate-x-full")
    : (open ? "translate-x-0" : "translate-x-full");
  const position = side === "left" ? "left-0" : "right-0";

  return (
    <div
      className={`absolute inset-y-0 ${position} z-21 transition-transform duration-200 ease-out ${translate}`}
    >
      <button
        onClick={onClose}
        className={`pointer-events-auto absolute top-1/2 z-20 -translate-y-1/2 rounded border border-slate-500 bg-slate-800 px-1 py-3 text-xs font-bold text-slate-200 transition-colors hover:bg-slate-700 hover:text-white ${
          side === "left" ? "right-0" : "left-0"
        }`}
      >
        {side === "left" ? "<" : ">"}
      </button>
      {children}
    </div>
  );
}

function ToggleButton({
  side,
  label,
  onClick,
}: {
  side: "left" | "right";
  label: string;
  onClick: () => void;
}) {
  const isLeft = side === "left";
  return (
    <button
      onClick={onClick}
      className={`pointer-events-auto absolute top-1/2 z-20 -translate-y-1/2 flex items-center gap-1 rounded border border-slate-500 bg-slate-800 px-1 py-3 text-xs font-bold text-slate-200 transition-colors hover:bg-slate-700 hover:text-white ${
        isLeft ? "left-0 rounded-l-none" : "right-0 rounded-r-none"
      }`}
    >
      <span
        className="text-[10px] uppercase tracking-wider text-slate-400"
        style={{ writingMode: "vertical-rl" }}
      >
        {label}
      </span>
      <span>{isLeft ? ">" : "<"}</span>
    </button>
  );
}

export function GamePage() {
  const currentView = useGameStateStore((s) => s.currentView);
  const [showLeft, setShowLeft] = useState(
    () => window.matchMedia("(min-width: 768px)").matches,
  );
  const [showRight, setShowRight] = useState(
    () => window.matchMedia("(min-width: 768px)").matches,
  );
  const isMap = currentView === "map";

  return (
    <div className="relative h-screen w-screen overflow-hidden">
      <TopBar />

      {isMap && (
        <div className="absolute inset-0">
          <MapCanvas />
        </div>
      )}

      {isMap && (
        <Panel side="left" open={showLeft} onClose={() => setShowLeft(false)}>
          <OverviewPanel />
        </Panel>
      )}
      {isMap && !showLeft && (
        <ToggleButton side="left" label="Status" onClick={() => setShowLeft(true)} />
      )}

      {isMap && (
        <Panel side="right" open={showRight} onClose={() => setShowRight(false)}>
          <VillagePanel />
        </Panel>
      )}
      {isMap && !showRight && (
        <ToggleButton side="right" label="Actions" onClick={() => setShowRight(true)} />
      )}

      {currentView !== "map" && (
        <div
          className="flex h-full bg-slate-950 bg-cover bg-top"
          style={{ backgroundImage: "url(/bg.png)" }}
        >
          <div className="flex flex-1 min-w-0 items-start overflow-y-auto pointer-events-auto">
            <div className="mx-auto flex w-full max-w-5xl h-full flex-col gap-2 p-4 pt-32">
              {currentView === "notifications" ? (
                <NotificationsPanel />
              ) : (
                <LeaderboardPanel />
              )}
            </div>
          </div>
        </div>
      )}

      <VillageTooltip />
    </div>
  );
}
