import type { MovementDto } from "../../api/types";
import { formatTime, timeRemaining } from "../../lib/helpers";
import { useTick } from "../../hooks/useTick";
import { ResourceCost } from "./ResourceCost";

function troopLabel(m: MovementDto): string {
  const parts: string[] = [];
  if (m.troops.swordsmen > 0) parts.push(`${m.troops.swordsmen} Swordsmen`);
  if (m.troops.archers > 0) parts.push(`${m.troops.archers} Archers`);
  if (m.troops.settlers > 0) parts.push(`${m.troops.settlers} Settlers`);
  return parts.join(", ") || "No troops";
}

const TYPE_LABEL: Record<string, string> = {
  Attack: "Attacking",
  Return: "Returning",
  Settle: "Settling",
  Transport: "Transporting",
};

export function MovementsPanel({
  villageId,
  movements,
}: {
  villageId: string;
  movements: readonly MovementDto[];
}) {
  useTick();

  const outgoing = movements.filter(
    (m) =>
      m.originVillageId === villageId &&
      m.status === "InFlight" &&
      m.type !== "Return",
  );
  const incoming = movements.filter(
    (m) =>
      m.status === "InFlight" &&
      ((m.targetVillageId === villageId && m.type !== "Return") ||
        (m.type === "Return" && m.originVillageId === villageId)),
  );

  if (outgoing.length === 0 && incoming.length === 0) return null;

  return (
    <section className=" border-slate-800 px-4 py-2">
      {/*<h3 className="mb-2 text-xs font-bold tracking-widest text-slate-400 uppercase">
        Movements
      </h3>*/}
      <div className="flex flex-col gap-1">
        {outgoing.map((m) => (
          <MovementRow key={m.id} movement={m} kind="outgoing" />
        ))}
        {incoming.map((m) => (
          <MovementRow key={m.id} movement={m} kind="incoming" />
        ))}
      </div>
    </section>
  );
}

function MovementRow({
  movement,
  kind,
}: {
  movement: MovementDto;
  kind: "outgoing" | "incoming";
}) {
  const color =
    movement.type === "Attack"
      ? "red"
      : movement.type === "Settle"
        ? "blue"
        : "green";
  const textColor =
    color === "red"
      ? "text-red-400"
      : color === "blue"
        ? "text-blue-400"
        : "text-green-400";
  const bgColor =
    color === "red"
      ? "bg-red-950/30"
      : color === "blue"
        ? "bg-blue-950/30"
        : "bg-green-950/30";

  const targetName =
    movement.targetVillageName ??
    (movement.targetCoordinates
      ? `${movement.targetCoordinates.x},${movement.targetCoordinates.y}`
      : null);

  return (
    <div
      className={`flex items-center justify-between rounded ${bgColor} px-2.5 py-1.5 text-xs`}
    >
      <div className="min-w-0 flex-1">
        <div className="flex items-center gap-1.5">
          <span className={`font-medium ${textColor}`}>
            {movement.type === "Attack" && kind === "incoming" ? "Incoming Attack" : TYPE_LABEL[movement.type] ?? movement.type}
          </span>
          <span className="truncate text-slate-400">
            {kind === "outgoing" ? (
              <>→ {targetName ?? "??"}</>
            ) : movement.type === "Return" ? (
              <>← Home</>
            ) : (
              <>← {movement.originVillageName}</>
            )}
          </span>
        </div>
        <div className="text-[10px] text-slate-500">{troopLabel(movement)}</div>
        {movement.carriedResources && (
          <ResourceCost value={movement.carriedResources} />
        )}
      </div>
      <span className="ml-2 shrink-0 whitespace-nowrap text-yellow-400">
        {formatTime(timeRemaining(movement.arrivesAt))}
      </span>
    </div>
  );
}
