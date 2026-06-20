import { useEffect } from "react";
import { useMyVillages } from "../api/hooks/useVillages";
import { useGameStateStore } from "../store/gameStateStore";

export function VillageList() {
  const { data: villages, isLoading, isError, error } = useMyVillages();
  const setVillages = useGameStateStore((s) => s.setVillages);
  const setActiveVillage = useGameStateStore((s) => s.setActiveVillage);
  const activeVillageId = useGameStateStore((s) => s.activeVillageId);

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
    <div className="flex-none h-screen w-72 flex-col border-r border-slate-800 bg-slate-950/90 font-sans text-white backdrop-blur-sm">
      <header className="border-b border-slate-800 px-4 py-3">
        <h2 className="text-xs font-bold tracking-widest text-slate-400 uppercase">
          Your Villages
        </h2>
      </header>

      <div className="flex-1 overflow-y-scroll">
        {isLoading && (
          <p className="px-4 py-3 text-sm text-slate-400">Loading…</p>
        )}

        {isError && (
          <p className="px-4 py-3 text-sm text-red-400">
            Failed to load villages
            {error instanceof Error ? `: ${error.message}` : ""}
          </p>
        )}

        {villages?.length === 0 && (
          <p className="px-4 py-3 text-sm text-slate-400">
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
                      "w-full cursor-pointer border-none px-4 py-3 text-left text-sm transition-colors",
                      isActive
                        ? "bg-blue-600 text-white"
                        : "bg-transparent text-white hover:bg-slate-800",
                    ].join(" ")}
                  >
                    <div className="font-semibold">{v.name}</div>
                    <div
                      className={[
                        "mt-1 text-xs",
                        isActive ? "text-blue-100" : "text-slate-400",
                      ].join(" ")}
                    >
                      Wood {v.resources.wood} · Clay {v.resources.clay} · Iron{" "}
                      {v.resources.iron} · Crop {v.resources.crop}
                    </div>
                  </button>
                </li>
              );
            })}
          </ul>
        )}
      </div>
    </div>
  );
}
