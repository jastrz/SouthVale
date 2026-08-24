import { useState } from "react";
import {
  useGameStateStore,
  selectActiveVillage,
} from "../../store/gameStateStore";
import {
  useVillage,
  useMyVillages,
  useMovements,
  useEmpire,
  useBuild,
  useTrain,
} from "../../api/hooks/useQueries";
import { PanelContainer } from "../PanelContainer";
import { Icon } from "../Icon";
import { UI_ICONS } from "../../lib/helpers";
import { CollapsibleSection } from "../CollapsibleSection";
import { BuildingsPanel } from "./BuildingsPanel";
import { TroopsPanel } from "./TroopsPanel";

export function VillagePanel() {
  const activeVillageId = useGameStateStore((s) => s.activeVillageId);
  const storeVillage = useGameStateStore(selectActiveVillage);

  if (!activeVillageId || !storeVillage) {
    return (
      <PanelContainer side="right">
        <div className="flex h-full items-center justify-center text-sm text-slate-500">
          No village selected
        </div>
      </PanelContainer>
    );
  }

  return (
    <VillagePanelInner
      villageId={activeVillageId}
    />
  );
}

function VillagePanelInner({
  villageId,
}: {
  villageId: string;
}) {
  const { data: village, isLoading, isError, error } = useVillage(villageId);
  const { data: myVillages } = useMyVillages();
  const { data: empire } = useEmpire();
  const buildMutation = useBuild(villageId);
  const trainMutation = useTrain(villageId);
  const { data: movements } = useMovements();
  const settlersInMovement = movements
    ?.filter(m => m.originVillageId === villageId && m.status === "InFlight")
    .reduce((sum, m) => sum + m.troops.settlers, 0) ?? 0;
  const [openBuildings, setOpenBuildings] = useState(true);
  const [openTroops, setOpenTroops] = useState(true);

  const hasBarracks = village?.buildings.some(
    (b) => (b.type === "Barracks" || b.type === "Stable") && b.level >= 1,
  );

  if (isLoading) {
    return (
      <PanelContainer side="right">
        <div className="flex h-full items-center justify-center text-sm text-slate-400">
          Loading village data…
        </div>
      </PanelContainer>
    );
  }

  if (isError || !village) {
    return (
      <PanelContainer side="right">
        <div className="flex h-full items-center justify-center text-sm text-red-400">
          {isError && error instanceof Error
            ? error.message
            : "Failed to load village"}
        </div>
      </PanelContainer>
    );
  }

  return (

    <PanelContainer side="right">
      <CollapsibleSection
        label={<span className="flex items-center gap-1.5"><Icon src={UI_ICONS.buildings} size={24} /> Buildings</span>}
        open={openBuildings}
        onToggle={() => setOpenBuildings(!openBuildings)}
        className="w-full px-4 pt-4 text-slate-400 hover:text-slate-300"
      >
        <BuildingsPanel
          buildings={village.buildings}
          buildOrders={village.buildOrders}
          mutation={buildMutation}
          resources={village.resources}
          empireStats={empire ?? undefined}
        />
      </CollapsibleSection>
      {hasBarracks && (
        <CollapsibleSection
          label={<span className="flex items-center gap-1.5"><Icon src={UI_ICONS.troops} size={24} /> Troops</span>}
          open={openTroops}
          onToggle={() => setOpenTroops(!openTroops)}
          className="w-full px-4 pt-1.5 text-slate-400 hover:text-slate-300"
        >
          <TroopsPanel
            key={villageId}
            resources={village.resources}
            troops={village.troops}
            buildings={village.buildings}
            mutation={trainMutation}
            villageCount={myVillages?.length ?? 1}
            settlersInTraining={village.trainOrders
              .filter(o => o.troopType === "Settler")
              .reduce((sum, o) => sum + o.amount - o.completed, 0)}
            settlersInMovement={settlersInMovement}
          />
        </CollapsibleSection>
      )}
    </PanelContainer>
  );
}
