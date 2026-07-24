import { useRef, useState } from "react";
import {
  useVillage,
  useTransport,
  useGameConfig,
} from "../../api/hooks/useQueries";
import { TravelTimeProvider } from "../travel-time/TravelTime";
import { TransportPanel } from "./TransportPanel";

export function TransportController({ villageId }: { villageId: string }) {
  const { data: village } = useVillage(villageId);
  const { data: gameConfig } = useGameConfig();
  const mutation = useTransport(villageId);
  const dialogRef = useRef<HTMLDialogElement>(null);
  const [openCount, setOpenCount] = useState(0);

  if (!village) return null;

  const troopSpeeds = gameConfig?.troops
    ? Object.fromEntries(
        Object.entries(gameConfig.troops).map(([k, v]) => [k, v.speed]),
      )
    : undefined;

  return (
    <TravelTimeProvider
      originX={village.coordinates.x}
      originY={village.coordinates.y}
      troopSpeeds={troopSpeeds}
      travelSpeedMultiplier={gameConfig?.travelSpeedMultiplier}
    >
      <button
        type="button"
        onClick={() => {
          setOpenCount((c) => c + 1);
          dialogRef.current?.showModal();
        }}
        className="mx-4 mb-2 mt-2 cursor-pointer rounded border border-emerald-700 bg-emerald-950/30 px-3 py-1 text-xs font-medium text-emerald-400 transition-colors hover:bg-emerald-900/50"
      >
        Send Transport
      </button>

      <dialog
        ref={dialogRef}
        className="w-full max-w-md rounded-xl bg-slate-800 p-0 text-white backdrop:bg-black/20 shadow-2xl"
        style={{ margin: "auto" }}
      >
        <TransportPanel
          key={openCount}
          villageId={villageId}
          troops={village.troops}
          resources={village.resources}
          mutation={mutation}
          onClose={() => dialogRef.current?.close()}
        />
      </dialog>
    </TravelTimeProvider>
  );
}
