import { useEffect } from "react";
import {
  useMyVillages,
  useMovements,
  useVillage,
  useCancelBuild,
  useCancelTrain,
} from "../api/hooks/useQueries";
import { useGameStateStore } from "../store/gameStateStore";
import { PanelContainer } from "./PanelContainer";
import { MovementsPanel } from "./village-panel/MovementsPanel";
import { QueuePanel } from "./village-panel/QueuePanel";
import { Icon } from "./Icon";
import { RESOURCE_ICONS } from "../lib/helpers";

export function OverviewPanel() {
  const { data: villages, isLoading, isError, error } = useMyVillages();
  const setVillages = useGameStateStore((s) => s.setVillages);
  const setActiveVillage = useGameStateStore((s) => s.setActiveVillage);
  const activeVillageId = useGameStateStore((s) => s.activeVillageId);
  const { data: movements } = useMovements();
  const { data: village } = useVillage(activeVillageId ?? "");
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
                      <div className="font-semibold">{v.name}</div>
                      <div
                        className={[
                          "mt-0.5 flex gap-1.5 text-[11px]",
                          isActive ? "text-blue-100" : "text-slate-400",
                        ].join(" ")}
                      >
                        <span className="flex items-center gap-0.5"><Icon src={RESOURCE_ICONS.wood} size={10} />{v.resources.wood}</span>
                        <span className="flex items-center gap-0.5"><Icon src={RESOURCE_ICONS.clay} size={10} />{v.resources.clay}</span>
                        <span className="flex items-center gap-0.5"><Icon src={RESOURCE_ICONS.iron} size={10} />{v.resources.iron}</span>
                        <span className="flex items-center gap-0.5"><Icon src={RESOURCE_ICONS.crop} size={10} />{v.resources.crop}</span>
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
            <MovementsPanel villageId={activeVillageId} movements={movements} />
          )}
        </div>
      </div>
    </PanelContainer>
  );
}
