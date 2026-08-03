import type { UseMutationResult } from "@tanstack/react-query";
import { BUILDING_LABELS, TROOP_LABELS } from "../../config/game";
import { formatTime, timeRemaining, BUILDING_ICONS, TROOP_ICONS } from "../../lib/helpers";
import { useTick } from "../../hooks/useTick";
import { Tooltip } from "../Tooltip";
import { Icon } from "../Icon";

function ProgressBar({ startMs, endMs, color = "bg-yellow-500" }: { startMs: number; endMs: number; color?: string }) {
  const total = endMs - startMs;
  const pct = total <= 0 ? 100 : Math.round(Math.min(1, Math.max(0, (Date.now() - startMs) / total)) * 100);
  return (
    <div className="mt-1 h-1 w-full overflow-hidden rounded bg-slate-700">
      <div
        className={`h-full rounded ${color} transition-[width] duration-1000 ease-linear`}
        style={{ width: `${pct}%` }}
      />
    </div>
  );
}

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
    startsAt: string;
    completesAt: string;
  }[];

  trainOrders: readonly {
    id: string;
    troopType: string;
    amount: number;
    completed: number;
    startedAt: string;
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

  const blockedCancelIds = new Set<string>();
  for (const o of buildOrders) {
    if (
      buildOrders.some(
        (b) => b.buildingType === o.buildingType && b.targetLevel > o.targetLevel,
      )
    ) {
      blockedCancelIds.add(o.id);
    }
  }

  return (
    <section className="px-4 py-3">
      {sortedBuilds.length > 0 && (
        <>
          <h3 className="mb-2 text-xs font-bold tracking-widest text-slate-400">
            Building
          </h3>
          {sortedBuilds.map((o, i) => (
            <div
              key={o.id}
              className="mb-1 flex items-center justify-between rounded bg-slate-800/50 px-2 py-1.5 text-xs text-slate-300"
            >
              <div className="flex-1">
                <span className="flex items-center gap-1 font-medium text-white">
                  {BUILDING_ICONS[o.buildingType] && (
                    <Icon src={BUILDING_ICONS[o.buildingType]} size={16} />
                  )}
                  {BUILDING_LABELS[o.buildingType] ?? o.buildingType}
                  <span className="text-slate-400">→ Lv.{o.targetLevel}</span>
                </span>
                <div className="mt-0.5 text-yellow-400">
                  {formatTime(timeRemaining(o.completesAt))}
                </div>
                {i === 0 && (
                  <ProgressBar
                    startMs={new Date(o.startsAt).getTime()}
                    endMs={new Date(o.completesAt).getTime()}
                  />
                )}
              </div>
              <div className="flex flex-col items-end gap-1">
                <span className="text-[10px] text-slate-500">
                  completes{" "}
                  {new Date(o.completesAt).toLocaleString(undefined, {
                    month: "short",
                    day: "numeric",
                    hour: "2-digit",
                    minute: "2-digit",
                  })}
                </span>
                {(() => {
                  const cancellable = !blockedCancelIds.has(o.id);
                  const btn = (
                    <button
                      onClick={() => cancelBuild.mutate(o.id)}
                      disabled={!cancellable || cancelBuild.isPending}
                      className="rounded px-1.5 py-0.5 text-xs text-red-400 hover:bg-red-900/30 hover:text-red-300 disabled:opacity-40 disabled:hover:bg-transparent disabled:hover:text-red-400"
                    >
                      cancel
                    </button>
                  );
                  return cancellable ? (
                    btn
                  ) : (
                    <Tooltip content="cancel the newest order for this building first">
                      {btn}
                    </Tooltip>
                  );
                })()}

              </div>
            </div>
          ))}
        </>
      )}
      {sortedTrains.length > 0 && (
        <>
          <h3 className="mb-2 mt-3 text-xs font-bold tracking-widest text-slate-400">
            Training
          </h3>
          {sortedTrains.map((o, i) => (
            <div
              key={o.id}
              className="mb-1 flex items-center justify-between rounded bg-slate-800/50 px-2 py-1.5 text-xs text-slate-300"
            >
              <div className="flex-1">
                <span className="flex items-center gap-1 font-medium text-white">
                  {TROOP_ICONS[o.troopType] && (
                    <Icon src={TROOP_ICONS[o.troopType]} size={16} />
                  )}
                  <span>{o.completed}/{o.amount}</span>
                  <span className="font-normal text-slate-300">
                    {TROOP_LABELS[o.troopType] ?? o.troopType}
                  </span>
                </span>
                <div className="mt-0.5 text-yellow-400">
                  {formatTime(timeRemaining(o.completesAt))}
                </div>
                {i === 0 && (
                  <ProgressBar
                    startMs={new Date(o.startedAt).getTime()}
                    endMs={new Date(o.completesAt).getTime()}
                  />
                )}
              </div>
              <div className="flex flex-col items-end gap-1">
                <span className="text-[10px] text-slate-500">
                  completes{" "}
                  {new Date(o.completesAt).toLocaleString(undefined, {
                    month: "short",
                    day: "numeric",
                    hour: "2-digit",
                    minute: "2-digit",
                  })}
                </span>
                <button
                  onClick={() => cancelTrain.mutate(o.id)}
                  disabled={cancelTrain.isPending}
                  className="rounded px-1.5 py-0.5 text-xs text-red-400 hover:bg-red-900/30 hover:text-red-300 disabled:opacity-40"
                >
                  cancel
                </button>
              </div>
            </div>
          ))}
        </>
      )}
    </section>
  );
}
