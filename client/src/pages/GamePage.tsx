import { useGameStateStore } from "../store/gameStateStore";
import { MapCanvas } from "../components/MapCanvas";
import { NotificationsPanel } from "../components/NotificationsPanel";
import { OverviewPanel } from "../components/OverviewPanel";
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
          {currentView === "map" ? <MapCanvas /> : <NotificationsPanel />}
        </div>
        {currentView === "map" && <VillagePanel />}
      </div>
      <VillageTooltip />
    </div>
  );
}
