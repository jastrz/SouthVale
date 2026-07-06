import { useGameStateStore } from "../store/gameStateStore";
import { MapCanvas } from "../components/MapCanvas";
import { NotificationsPanel } from "../components/NotificationsPanel";
import { LeaderboardPanel } from "../components/LeaderboardPanel";
import { OverviewPanel } from "../components/overview-panel/OverviewPanel";
import { VillagePanel } from "../components/village-panel";
import { VillageTooltip } from "../components/VillageTooltip";
import { TopBar } from "../components/TopBar";

export function GamePage() {
  const currentView = useGameStateStore((s) => s.currentView);

  return (
    <div className="relative h-screen w-screen">
      <TopBar />

      {currentView === "map" && (
        <div className="absolute inset-0">
          <MapCanvas />
        </div>
      )}

      {currentView === "map" && (
        <div className="absolute inset-y-0 left-0 z-10">
          <OverviewPanel />
        </div>
      )}

      {currentView === "map" && (
        <div className="absolute inset-y-0 right-0 z-10">
          <VillagePanel />
        </div>
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
