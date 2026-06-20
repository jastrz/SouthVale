import { MapCanvas } from "../components/MapCanvas";
import { VillageList } from "../components/VillageList";
import { VillagePanel } from "../components/village-panel";
import { VillageTooltip } from "../components/VillageTooltip";

export function GamePage() {
  return (
    <div className="flex h-screen w-screen">
      <VillageList />
      <div className="flex flex-1 min-w-0 items-center">
        <MapCanvas />
      </div>
      <VillagePanel />
      <VillageTooltip />
    </div>
  );
}
