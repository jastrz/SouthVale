import { useEffect } from "react";
import {
  useMyVillages,
  useMovements,
  useVillage,
  useVillageStatus,
  useCancelBuild,
  useCancelTrain,
} from "../../api/hooks/useQueries";
import { useGameStateStore } from "../../store/gameStateStore";
import { PanelContainer } from "../PanelContainer";
import { VillageListItem } from "./VillageListItem";
import { MovementsPanel } from "../village-panel/MovementsPanel";
import { QueuePanel } from "../village-panel/QueuePanel";
import { TransportController } from "../transport/TransportController";

export function OverviewPanel() {
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
      <div className="flex h-full w-72 flex-col border-r border-slate-800 bg-slate-950/90 font-sans text-white backdrop-blur-sm">
        <header className="border-b border-slate-800 px-4 py-2">
          <h2 className="text-xs font-bold tracking-widest text-slate-400 uppercase">
            Your Villages
          </h2>
        </header>

        <div className="flex-1 overflow-y-auto min-h-0">
          {isLoading && (
            <p className="px-4 py-2 text-sm text-slate-400">Loading…</p>
          )}

          {isError && (
            <p className="px-4 py-2 text-sm text-red-400">
              Failed to load villages
              {error instanceof Error ? `: ${error.message}` : ""}
            </p>
          )}

          {villages?.length === 0 && (
            <p className="px-4 py-2 text-sm text-slate-400">
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
        </div>
        <div className="flex-1 overflow-y-auto min-h-0">
          {village && (
            <QueuePanel
              buildOrders={village.buildOrders}
              trainOrders={village.trainOrders}
              cancelBuild={cancelBuild}
              cancelTrain={cancelTrain}
            />
          )}
          {movements && movements.length > 0 && (
            <MovementsPanel
              villageId={activeVillageId!}
              movements={movements}
            />
          )}
        </div>
        {activeVillageId && <TransportController villageId={activeVillageId} />}
      </div>
    </PanelContainer>
  );
}
