import { BUILDING_LABELS, TROOP_LABELS } from "../../config/game";
import { formatTime, timeRemaining } from "./helpers";
import { useTick } from "../../hooks/useTick";

export function QueuePanel({
  buildOrders,
  trainOrders,
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
}) {
  useTick();

  if (buildOrders.length === 0 && trainOrders.length === 0) return null;

  return (
    <section className="px-4 py-3">
      <h3 className="mb-2 text-xs font-bold tracking-widest text-slate-400 uppercase">
        Active Orders
      </h3>
      {buildOrders.map((o) => (
        <div
          key={o.id}
          className="mb-1 rounded bg-slate-800/50 px-2 py-1.5 text-xs text-slate-300"
        >
          <span className="font-medium text-white">
            {BUILDING_LABELS[o.buildingType] ?? o.buildingType}
          </span>{" "}
          → Lv.{o.targetLevel}
          <span className="ml-2 text-yellow-400">
            {formatTime(timeRemaining(o.completesAt))}
          </span>
        </div>
      ))}
      {trainOrders.map((o) => (
        <div
          key={o.id}
          className="mb-1 rounded bg-slate-800/50 px-2 py-1.5 text-xs text-slate-300"
        >
          <span className="font-medium text-white">
            {o.completed}/{o.amount}
          </span>{" "}
          {TROOP_LABELS[o.troopType] ?? o.troopType}
          <span className="ml-2 text-yellow-400">
            {formatTime(timeRemaining(o.completesAt))}
          </span>
        </div>
      ))}
    </section>
  );
}
