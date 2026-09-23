import { useWorldStatus } from "../../api/hooks/useQueries";
import { formatTime, timeRemaining } from "../../lib/helpers";

const MEDAL_STYLES = [
  "bg-amber-400 text-slate-900",
  "bg-slate-300 text-slate-900",
  "bg-amber-700 text-white",
];

export function WorldInfoPanel() {
  const { data } = useWorldStatus();
  if (!data) return null;

  const remaining = data.endsAt ? timeRemaining(data.endsAt) : null;
  const resetIn =
    remaining === null
      ? null
      : remaining > 0
        ? formatTime(remaining)
        : "any moment now";
  const resetAt = data.endsAt
    ? new Date(data.endsAt)
        .toLocaleString(undefined, {
          day: "numeric",
          month: "short",
          hour: "2-digit",
          minute: "2-digit",
          hour12: false,
        })
        .toLowerCase()
    : null;

  return (
    <div className="flex w-full max-w-sm flex-col gap-4 rounded-xl bg-slate-800/80 px-4 py-3 text-xs text-slate-300">
      <p className="text-center">
        <span className="font-bold tracking-wider text-white">
          World {data.iteration}
        </span>
        {resetIn && (
          <>
            {" "}· resets in <span className="text-amber-500">{resetIn}</span>
          </>
        )}
        {resetAt && (
          <span className="text-[10px] text-slate-400"> ({resetAt})</span>
        )}
      </p>

      {data.previous && data.previous.winners.length > 0 && (
        <div className="flex flex-col gap-1.5">
          <p className="text-center text-[10px] uppercase tracking-widest text-slate-400">
            World {data.previous.iteration} winners
          </p>
          <div className="flex flex-col divide-y divide-slate-700">
            {data.previous.winners.map((w, i) => (
              <div key={w.username} className="flex items-center gap-2 py-1.5">
                <span
                  className={`flex h-4 w-4 shrink-0 items-center justify-center rounded-full text-[10px] font-bold ${MEDAL_STYLES[i] ?? "bg-slate-600 text-white"}`}
                >
                  {i + 1}
                </span>
                <span className="flex-1 truncate text-white">{w.username}</span>
                <span className="tabular-nums">{w.score.toLocaleString()}</span>
              </div>
            ))}
          </div>
        </div>
      )}

      {data.bestEver && (
        <p className="border-t border-slate-700 pt-3 text-center text-[11px] text-slate-400">
          Best ever: <span className="text-white">{data.bestEver.username}</span>{" "}
          <span className="text-amber-500">
            {data.bestEver.score.toLocaleString()}
          </span>{" "}
          (World {data.bestEver.iteration})
        </p>
      )}
    </div>
  );
}
