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
      <div className="flex h-full">
        <OverviewPanel />
        <div className="flex flex-1 min-w-0 items-start overflow-y-auto">
          <div className="flex w-full h-full flex-col pt-12">
            {currentView === "map" ? (
              <MapCanvas />
            ) : (
              <div className="mx-auto flex w-full max-w-5xl h-full flex-col gap-2 p-4 pt-20">
                {currentView === "notifications" ? (
                  <NotificationsPanel />
                ) : (
                  <LeaderboardPanel />
                )}
              </div>
            )}
          </div>
        </div>
        {currentView === "map" && <VillagePanel />}
      </div>
      <VillageTooltip />
    </div>
  );
}
