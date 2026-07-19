import type { UseMutationResult } from "@tanstack/react-query";
import { BUILDING_LABELS, TROOP_LABELS } from "../../config/game";
import { formatTime, timeRemaining } from "../../lib/helpers";
import { useTick } from "../../hooks/useTick";

export function QueuePanel({
  buildOrders,
  trainOrders,
  cancelBuild,
  cancelTrain,
}: {
  buildOrders: readonly {
    id: string;
    buildingType: string;
    targetLevel: number;
    completesAt: string;
  }[];

  trainOrders: readonly {
    id: string;
    troopType: string;
    amount: number;
    completed: number;
    completesAt: string;
  }[];

  // doesn't read .data or .error, add exact type if it ever does
  cancelBuild: UseMutationResult<unknown, unknown, string>;
  cancelTrain: UseMutationResult<unknown, unknown, string>;
}) {
  useTick();

  const sortedBuilds = [...buildOrders].sort(
    (a, b) => new Date(a.completesAt).getTime() - new Date(b.completesAt).getTime(),
  );
  const sortedTrains = [...trainOrders].sort(
    (a, b) => new Date(a.completesAt).getTime() - new Date(b.completesAt).getTime(),
  );

  return (
    <section className="px-4 py-3">
      {sortedBuilds.length > 0 && (
        <>
          <h3 className="mb-2 text-xs font-bold tracking-widest text-slate-400 uppercase">
            Building
          </h3>
          {sortedBuilds.map((o) => (
            <div
              key={o.id}
              className="mb-1 flex items-center justify-between rounded bg-slate-800/50 px-2 py-1.5 text-xs text-slate-300"
            >
              <div>
                <span className="font-medium text-white">
                  {BUILDING_LABELS[o.buildingType] ?? o.buildingType}
                </span>{" "}
                → Lv.{o.targetLevel}
                <div className="mt-0.5 text-yellow-400">
                  {formatTime(timeRemaining(o.completesAt))}
                </div>
              </div>
              <button
                onClick={() => cancelBuild.mutate(o.id)}
                disabled={cancelBuild.isPending}
                className="ml-2 rounded px-1.5 py-0.5 text-xs text-red-400 hover:bg-red-900/30 hover:text-red-300 disabled:opacity-40"
              >
                cancel
              </button>
            </div>
          ))}
        </>
      )}
      {sortedTrains.length > 0 && (
        <>
          <h3 className="mb-2 mt-3 text-xs font-bold tracking-widest text-slate-400 uppercase">
            Training
          </h3>
          {sortedTrains.map((o) => (
            <div
              key={o.id}
              className="mb-1 flex items-center justify-between rounded bg-slate-800/50 px-2 py-1.5 text-xs text-slate-300"
            >
              <div>
                <span className="font-medium text-white">
                  {o.completed}/{o.amount}
                </span>{" "}
                {TROOP_LABELS[o.troopType] ?? o.troopType}
                <div className="mt-0.5 text-yellow-400">
                  {formatTime(timeRemaining(o.completesAt))}
                </div>
              </div>
              <button
                onClick={() => cancelTrain.mutate(o.id)}
                disabled={cancelTrain.isPending}
                className="ml-2 rounded px-1.5 py-0.5 text-xs text-red-400 hover:bg-red-900/30 hover:text-red-300 disabled:opacity-40"
              >
                cancel
              </button>
            </div>
          ))}
        </>
      )}
    </section>
  );
}
