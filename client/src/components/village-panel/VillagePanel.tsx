import { useState } from "react";
import {
  useGameStateStore,
  selectActiveVillage,
} from "../../store/gameStateStore";
import {
  useVillage,
  useMyVillages,
  useMovements,
  useBuild,
  useTrain,
  useAttack,
  useSettle,
  useGameConfig,
} from "../../api/hooks/useQueries";
import type { MapVillage } from "../../api/types";
import { PanelContainer } from "../PanelContainer";
import { CollapsibleSection } from "../CollapsibleSection";
import { BuildingsPanel } from "./BuildingsPanel";
import { TroopsPanel } from "./TroopsPanel";
import { AttackPanel } from "./AttackPanel";
import { SettlePanel } from "./SettlePanel";
import { TravelTimeProvider } from "../travel-time/TravelTime";

export function VillagePanel() {
  const activeVillageId = useGameStateStore((s) => s.activeVillageId);
  const storeVillage = useGameStateStore(selectActiveVillage);
  const targetVillage = useGameStateStore((s) => s.targetVillage);
  const selectedTile = useGameStateStore((s) => s.selectedTile);

  if (!activeVillageId || !storeVillage) {
    return (
      <PanelContainer>
        <div className="flex h-full items-center justify-center text-sm text-slate-500">
          No village selected
        </div>
      </PanelContainer>
    );
  }

  return (
    <VillagePanelInner
      villageId={activeVillageId}
      targetVillage={targetVillage}
      selectedTile={selectedTile}
    />
  );
}

function VillagePanelInner({
  villageId,
  targetVillage,
  selectedTile,
}: {
  villageId: string;
  targetVillage: MapVillage | null;
  selectedTile: { x: number; y: number } | null;
}) {
  const { data: village, isLoading, isError, error } = useVillage(villageId);
  const { data: config } = useGameConfig();
  const { data: myVillages } = useMyVillages();
  const troopSpeeds = config?.troops
    ? Object.fromEntries(Object.entries(config.troops).map(([k, v]) => [k, v.speed]))
    : undefined;
  const buildMutation = useBuild(villageId);
  const trainMutation = useTrain(villageId);
  const { data: movements } = useMovements();
  const settlersInMovement = movements
    ?.filter(m => m.originVillageId === villageId && m.status === "InFlight")
    .reduce((sum, m) => sum + m.troops.settlers, 0) ?? 0;
  const attackMutation = useAttack(villageId);
  const settleMutation = useSettle(villageId);
  const setTargetVillage = useGameStateStore((s) => s.setTargetVillage);
  const setSelectedTile = useGameStateStore((s) => s.setSelectedTile);
  const [openBuildings, setOpenBuildings] = useState(true);
  const [openTroops, setOpenTroops] = useState(true);

  const hasBarracks = village?.buildings.some(
    (b) => (b.type === "Barracks" || b.type === "Stable") && b.level >= 1,
  );

  if (isLoading) {
    return (
      <PanelContainer>
        <div className="flex h-full items-center justify-center text-sm text-slate-400">
          Loading village data…
        </div>
      </PanelContainer>
    );
  }

  if (isError || !village) {
    return (
      <PanelContainer>
        <div className="flex h-full items-center justify-center text-sm text-red-400">
          {isError && error instanceof Error
            ? error.message
            : "Failed to load village"}
        </div>
      </PanelContainer>
    );
  }

  return (

    <PanelContainer>
      <CollapsibleSection
        label="Buildings"
        open={openBuildings}
        onToggle={() => setOpenBuildings(!openBuildings)}
        className="px-4 pt-4 text-slate-400 hover:text-slate-300"
      >
        <BuildingsPanel
          buildings={village.buildings}
          buildOrders={village.buildOrders}
          mutation={buildMutation}
          resources={village.resources}
        />
      </CollapsibleSection>
      {hasBarracks && (
        <CollapsibleSection
          label="Troops"
          open={openTroops}
          onToggle={() => setOpenTroops(!openTroops)}
          className="px-4 pt-1.5 text-slate-400 hover:text-slate-300"
        >
          <TroopsPanel
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
      <TravelTimeProvider
        originX={village.coordinates.x}
        originY={village.coordinates.y}
        troopSpeeds={troopSpeeds}
        travelSpeedMultiplier={config?.travelSpeedMultiplier}
      >
{targetVillage && targetVillage.kind === "enemy" && (
          <AttackPanel
            targetName={targetVillage.name}
            targetX={targetVillage.coordinates.x}
            targetY={targetVillage.coordinates.y}
            targetPopulation={targetVillage.population}
            targetVillageId={targetVillage.id}
            maxSwordsmen={village.troops.swordsmen}
            maxArchers={village.troops.archers}
            maxDogs={village.troops.dogs}
            maxHorsemen={village.troops.horsemen}
            maxLlamaRiders={village.troops.llamaRiders}
            mutation={attackMutation}
            onClearTarget={() => setTargetVillage(null)}
          />
        )}

        {selectedTile && (
          <SettlePanel
            targetX={selectedTile.x}
            targetY={selectedTile.y}
            settlers={village.troops.settlers}
            villageCount={myVillages?.length ?? 1}
            maxVillages={config?.maxVillagesPerPlayer ?? 8}
            mutation={settleMutation}
            onClearTarget={() => setSelectedTile(null)}
          />
        )}

      </TravelTimeProvider>
    </PanelContainer>
  );
}
