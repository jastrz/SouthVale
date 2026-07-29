import { useState } from "react";
import { useGameStateStore } from "../store/gameStateStore";
import { MapCanvas } from "../components/MapCanvas";
import { NotificationsPanel } from "../components/NotificationsPanel";
import { LeaderboardPanel } from "../components/LeaderboardPanel";
import { OverviewPanel } from "../components/overview-panel/OverviewPanel";
import { VillagePanel } from "../components/village-panel";
// import { VillageTooltip } from "../components/VillageTooltip";
import { VillagePopup } from "../components/VillagePopup";
import { TilePopup } from "../components/TilePopup";
import { GameInfoPanel } from "../components/GameInfoPanel";
import { TopBar } from "../components/TopBar";
import { PANEL_CLAMP } from "../pixi/config";

function Panel({
  side,
  open,
  children,
}: {
  side: "left" | "right";
  open: boolean;
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
      {children}
    </div>
  );
}

function PanelTab({
  side,
  open,
  label,
  onToggle,
}: {
  side: "left" | "right";
  open: boolean;
  label: string;
  onToggle: () => void;
}) {
  const isLeft = side === "left";
  return (
    <button
      onClick={onToggle}
      className={`pointer-events-auto absolute top-1/2 z-22 -translate-y-1/2 flex items-center gap-1 rounded-xl border border-slate-500 bg-slate-800 px-1 py-3 text-xs font-bold text-slate-200 transition-colors hover:bg-slate-700 hover:text-white ${
        isLeft ? "rounded-l-none" : "rounded-r-none"
      }`}
      style={{ [isLeft ? "left" : "right"]: open ? PANEL_CLAMP : 0 }}
    >
      {open ? (
        <span>{isLeft ? "<" : ">"}</span>
      ) : (
        <>
          <span
            className="text-[10px] uppercase tracking-wider text-slate-400"
            style={{ writingMode: "vertical-rl" }}
          >
            {label}
          </span>
          <span>{isLeft ? ">" : "<"}</span>
        </>
      )}
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
    <div className="relative w-screen overflow-hidden" style={{ height: "100dvh" }}>
      <TopBar />

      {isMap && (
        <div className="absolute inset-0">
          <MapCanvas />
        </div>
      )}

      {isMap && (
        <>
          <Panel side="left" open={showLeft}>
            <OverviewPanel />
          </Panel>
          <PanelTab side="left" open={showLeft} label="Status" onToggle={() => setShowLeft(!showLeft)} />
        </>
      )}

      {isMap && (
        <>
          <Panel side="right" open={showRight}>
            <VillagePanel />
          </Panel>
          <PanelTab side="right" open={showRight} label="Actions" onToggle={() => setShowRight(!showRight)} />
        </>
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
              ) : currentView === "leaderboard" ? (
                <LeaderboardPanel pageSize={20} />
              ) : (
                <GameInfoPanel />
              )}
            </div>
          </div>
        </div>
      )}

      {/* <VillageTooltip /> */}
      <VillagePopup />
      <TilePopup />
    </div>
  );
}
