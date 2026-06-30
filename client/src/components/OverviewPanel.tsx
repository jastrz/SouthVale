import { useEffect } from "react";
import {
  useMyVillages,
  useMovements,
  useVillage,
  useVillageStatus,
  useCancelBuild,
  useCancelTrain,
} from "../api/hooks/useQueries";
import { useGameStateStore } from "../store/gameStateStore";
import { PanelContainer } from "./PanelContainer";
import { MovementsPanel } from "./village-panel/MovementsPanel";
import { QueuePanel } from "./village-panel/QueuePanel";
import { Icon } from "./Icon";
import { RESOURCE_ICONS, TROOP_ICONS } from "../lib/helpers";

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
              {villages.map((v) => {
                const isActive = v.id === activeVillageId;
                const inc = incomingCountMap?.[v.id] ?? 0;
                const s = statusMap.get(v.id);
                const mc = movementCountMap?.[v.id] ?? 0;
                const bc = s?.buildOrderCount ?? 0;
                const tc = s?.trainOrderCount ?? 0;
                const r = s?.resources ?? v.resources;
                const t = s?.troops ?? v.troops;
                return (
                  <li key={v.id}>
                    <button
                      type="button"
                      onClick={() => setActiveVillage(v.id)}
                      aria-current={isActive ? "true" : undefined}
                      className={[
                        "w-full cursor-pointer border-none px-4 py-2 text-left text-sm transition-colors",
                        isActive
                          ? "bg-blue-600 text-white"
                          : "bg-transparent text-white hover:bg-slate-800",
                      ].join(" ")}
                    >
                      <div className="flex items-center gap-2">
                        <span className="font-semibold">{v.name}</span>
                        {(bc > 0 || tc > 0 || mc > 0 || inc > 0) && (
                          <span className="flex gap-1 text-[10px] text-amber-400">
                            {inc > 0 && (
                              <span className="text-red-400">A:{inc}</span>
                            )}
                            {bc > 0 && <span>B:{bc}</span>}
                            {tc > 0 && <span>T:{tc}</span>}
                            {mc > 0 && <span>M:{mc}</span>}
                          </span>
                        )}
                      </div>
                      <div
                        className={[
                          "mt-0.5 flex gap-1.5 text-[11px]",
                          isActive ? "text-blue-100" : "text-slate-400",
                        ].join(" ")}
                      >
                        <span className="flex items-center gap-0.5">
                          <Icon src={RESOURCE_ICONS.wood} size={10} />
                          {r.wood}
                        </span>
                        <span className="flex items-center gap-0.5">
                          <Icon src={RESOURCE_ICONS.clay} size={10} />
                          {r.clay}
                        </span>
                        <span className="flex items-center gap-0.5">
                          <Icon src={RESOURCE_ICONS.iron} size={10} />
                          {r.iron}
                        </span>
                        <span className="flex items-center gap-0.5">
                          <Icon src={RESOURCE_ICONS.crop} size={10} />
                          {r.crop}
                        </span>
                      </div>
                      <div
                        className={[
                          "flex gap-1.5 text-[11px]",
                          isActive ? "text-blue-100" : "text-slate-400",
                        ].join(" ")}
                      >
                        <span className="flex items-center gap-0.5">
                          <Icon src={TROOP_ICONS.Swordsman} size={10} />
                          {t.swordsmen}
                        </span>
                        <span className="flex items-center gap-0.5">
                          <Icon src={TROOP_ICONS.Archer} size={10} />
                          {t.archers}
                        </span>
                        <span className="flex items-center gap-0.5">
                          <Icon src={TROOP_ICONS.Settler} size={10} />
                          {t.settlers}
                        </span>
                      </div>
                    </button>
                  </li>
                );
              })}
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
      </div>
    </PanelContainer>
  );
}
