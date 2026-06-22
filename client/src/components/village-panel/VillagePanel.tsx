import {
  useGameStateStore,
  selectActiveVillage,
} from "../../store/gameStateStore";
import {
  useVillage,
  useBuild,
  useTrain,
  useAttack,
  useSettle,
  useMovements,
  useCancelBuild,
  useCancelTrain,
} from "../../api/hooks/useVillages";
import type { MapVillage } from "../../api/types";
import { PanelContainer } from "../PanelContainer";
import { VillageHeader } from "./VillageHeader";
import { ResourceDisplay } from "./ResourceDisplay";
import { BuildingsPanel } from "./BuildingsPanel";
import { TroopsPanel } from "./TroopsPanel";
import { QueuePanel } from "./QueuePanel";
import { AttackPanel } from "./AttackPanel";
import { SettlePanel } from "./SettlePanel";
import { MovementsPanel } from "./MovementsPanel";

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
  const buildMutation = useBuild(villageId);
  const trainMutation = useTrain(villageId);
  const attackMutation = useAttack(villageId);
  const settleMutation = useSettle(villageId);
  const cancelBuildMutation = useCancelBuild(villageId);
  const cancelTrainMutation = useCancelTrain(villageId);
  const { data: movements } = useMovements();
  const setTargetVillage = useGameStateStore((s) => s.setTargetVillage);
  const setSelectedTile = useGameStateStore((s) => s.setSelectedTile);

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
      <VillageHeader
        name={village.name}
        x={village.coordinates.x}
        y={village.coordinates.y}
      />
      <ResourceDisplay
        wood={Math.floor(village.resources.wood)}
        clay={Math.floor(village.resources.clay)}
        iron={Math.floor(village.resources.iron)}
        crop={Math.floor(village.resources.crop)}
      />
      <BuildingsPanel
        buildings={village.buildings}
        buildOrders={village.buildOrders}
        mutation={buildMutation}
      />
      <TroopsPanel
        swordsmen={village.troops.swordsmen}
        archers={village.troops.archers}
        settlers={village.troops.settlers}
        mutation={trainMutation}
      />
      <QueuePanel
        buildOrders={village.buildOrders}
        trainOrders={village.trainOrders}
        cancelBuild={cancelBuildMutation}
        cancelTrain={cancelTrainMutation}
      />
      {targetVillage && targetVillage.kind === "enemy" && (
        <AttackPanel
          targetName={targetVillage.name}
          targetX={targetVillage.coordinates.x}
          targetY={targetVillage.coordinates.y}
          targetPopulation={targetVillage.population}
          targetVillageId={targetVillage.id}
          maxSwordsmen={village.troops.swordsmen}
          maxArchers={village.troops.archers}
          mutation={attackMutation}
          onClearTarget={() => setTargetVillage(null)}
        />
      )}

      {selectedTile && (
        <SettlePanel
          targetX={selectedTile.x}
          targetY={selectedTile.y}
          settlers={village.troops.settlers}
          mutation={settleMutation}
          onClearTarget={() => setSelectedTile(null)}
        />
      )}

      <MovementsPanel villageId={villageId} movements={movements ?? []} />
    </PanelContainer>
  );
}
