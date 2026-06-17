import { MapCanvas } from "../components/MapCanvas";
import { VillageList } from "../components/VillageList";

export function GamePage() {
  return (
    <div className="flex h-screen w-screen">
      <VillageList />
      <div className="flex flex-1 items-center">
        <MapCanvas />
      </div>
    </div>
  );
}
