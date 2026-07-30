import { useEffect, useState } from "react";
import {
  useMyVillages,
  useMovements,
  useVillage,
  useVillageStatus,
  useCancelBuild,
  useCancelTrain,
} from "../../api/hooks/useQueries";
import { useGameStateStore } from "../../store/gameStateStore";
import { Icon } from "../Icon";
import { UI_ICONS } from "../../lib/helpers";
import { CollapsibleSection } from "../CollapsibleSection";
import { PanelContainer } from "../PanelContainer";
import { VillageListItem } from "./VillageListItem";
import { MovementsPanel } from "../village-panel/MovementsPanel";
import { QueuePanel } from "../village-panel/QueuePanel";
import { TransportController } from "../transport/TransportController";
import { TradeController } from "../trade/TradeController";

export function OverviewPanel() {
  const [openVillages, setOpenVillages] = useState(true);
  const [openOrders, setOpenOrders] = useState(true);
  const [openMovements, setOpenMovements] = useState(true);
  const { data: villages, isLoading, isError, error } = useMyVillages();
  const setVillages = useGameStateStore((s) => s.setVillages);
  const setActiveVillage = useGameStateStore((s) => s.setActiveVillage);
  const activeVillageId = useGameStateStore((s) => s.activeVillageId);
  const { data: movements } = useMovements();
  const { data: statuses } = useVillageStatus();
  const { data: village } = useVillage(activeVillageId ?? "");

  const statusMap = new Map(statuses?.map((s) => [s.villageId, s]));

  const movementCountMap = movements?.reduce(
    (acc, m) => {
      acc[m.originVillageId] = (acc[m.originVillageId] ?? 0) + 1;
      return acc;
    },
    {} as Record<string, number>,
  );

  const incomingCountMap = movements?.reduce(
    (acc, m) => {
      if (m.status === "InFlight" && m.targetVillageId && m.type === "Attack") {
        acc[m.targetVillageId] = (acc[m.targetVillageId] ?? 0) + 1;
      }
      return acc;
    },
    {} as Record<string, number>,
  );

  const cancelBuild = useCancelBuild(activeVillageId ?? "");
  const cancelTrain = useCancelTrain(activeVillageId ?? "");

  useEffect(() => {
    if (villages) setVillages(villages);
  }, [villages, setVillages]);

  useEffect(() => {
    if (!villages || villages.length === 0) return;
    if (activeVillageId && villages.some((v) => v.id === activeVillageId))
      return;
    setActiveVillage(villages[0].id);
  }, [villages, activeVillageId, setActiveVillage]);

  return (
    <PanelContainer>
      <div className="flex h-full w-full flex-col font-sans text-white">
        <div className="flex-1 overflow-y-auto min-h-0">
          <CollapsibleSection
            label={<span className="flex items-center gap-1.5"><Icon src={UI_ICONS.village} size={24} /> Villages</span>}
            open={openVillages}
            onToggle={() => setOpenVillages(!openVillages)}
            className="w-full px-4 pt-1.5 text-slate-400 hover:text-slate-300"
          >
            {isLoading && (
              <p className="text-sm text-slate-400">Loading…</p>
            )}

            {isError && (
              <p className="text-sm text-red-400">
                Failed to load villages
                {error instanceof Error ? `: ${error.message}` : ""}
              </p>
            )}

            {villages?.length === 0 && (
              <p className="text-sm text-slate-400">
                You don&apos;t have any villages yet.
              </p>
            )}

            {villages && villages.length > 0 && (
              <ul className="flex flex-col">
                {villages.map((v) => (
                  <VillageListItem
                    key={v.id}
                    village={v}
                    status={statusMap.get(v.id)}
                    isActive={v.id === activeVillageId}
                    incomingCount={incomingCountMap?.[v.id] ?? 0}
                    movementCount={movementCountMap?.[v.id] ?? 0}
                    onClick={() => setActiveVillage(v.id)}
                  />
                ))}
              </ul>
            )}
          </CollapsibleSection>

          <div className="h-4" />

          {village && (village.buildOrders.length > 0 || village.trainOrders.length > 0) && (
            <CollapsibleSection
              label="Orders"
              open={openOrders}
              onToggle={() => setOpenOrders(!openOrders)}
              className="w-full px-4 pt-1.5 text-slate-400 hover:text-slate-300"
            >
              <QueuePanel
                buildOrders={village.buildOrders}
                trainOrders={village.trainOrders}
                cancelBuild={cancelBuild}
                cancelTrain={cancelTrain}
              />
            </CollapsibleSection>
          )}
          {activeVillageId && (movements?.filter((m) => m.originVillageId === activeVillageId || m.targetVillageId === activeVillageId)?.length ?? 0) > 0 && (
            <CollapsibleSection
              label="Movements"
              open={openMovements}
              onToggle={() => setOpenMovements(!openMovements)}
              className="w-full px-4 pt-1.5 text-slate-400 hover:text-slate-300"
            >
              <MovementsPanel
                villageId={activeVillageId}
                movements={movements ?? []}
              />
            </CollapsibleSection>
          )}
          {activeVillageId && <TransportController villageId={activeVillageId} />}
          {activeVillageId && <TradeController villageId={activeVillageId} />}
        </div>
      </div>
    </PanelContainer>
  );
}
