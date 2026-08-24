import { useState } from "react";
import { Button } from "../Button";
import {
  useVillage,
  useTransport,
  useGameConfig,
} from "../../api/hooks/useQueries";
import { TravelTimeProvider } from "../travel-time/TravelTime";
import { Modal } from "../Modal";
import { TransportPanel } from "./TransportPanel";

export function TransportController({ villageId }: { villageId: string }) {
  const { data: village } = useVillage(villageId);
  const { data: gameConfig } = useGameConfig();
  const mutation = useTransport(villageId);
  const [open, setOpen] = useState(false);

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
      <Button
        type="button"
        onClick={() => setOpen(true)}
        className="mx-4 mb-2 mt-2 cursor-pointer rounded border border-emerald-700 bg-emerald-950/30 px-3 py-1 text-xs font-medium text-emerald-400 transition-colors hover:bg-emerald-900/50"
      >
        Send Transport
      </Button>

      <Modal open={open} onClose={() => setOpen(false)}>
        <TransportPanel
          villageId={villageId}
          troops={village.troops}
          resources={village.resources}
          mutation={mutation}
          onClose={() => setOpen(false)}
        />
      </Modal>
    </TravelTimeProvider>
  );
}
