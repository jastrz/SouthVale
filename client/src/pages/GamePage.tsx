import { MapCanvas } from "../components/MapCanvas";
import { VillageList } from "../components/VillageList";
import { VillagePanel } from "../components/village-panel";
import { VillageTooltip } from "../components/VillageTooltip";
import { TopBar } from "../components/TopBar";

export function GamePage() {
  return (
    <div className="relative h-screen w-screen">
      <TopBar />
      <div className="flex h-full">
        <VillageList />
        <div className="flex flex-1 min-w-0 items-center">
          <MapCanvas />
        </div>
        <VillagePanel />
      </div>
      <VillageTooltip />
    </div>
  );
}
