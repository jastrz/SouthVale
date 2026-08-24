import { useState } from "react";
import { Button } from "../Button";
import { useVillage, useTrade, useGameConfig } from "../../api/hooks/useQueries";
import { Modal } from "../Modal";
import { TradePanel } from "./TradePanel";

export function TradeController({ villageId }: { villageId: string }) {
  const { data: village } = useVillage(villageId);
  const { data: config } = useGameConfig();
  const mutation = useTrade(villageId);
  const [open, setOpen] = useState(false);

  if (!village) return null;

  const tradePost = village.buildings.find((b) => b.type === "TradePost");
  if (!tradePost || tradePost.level < 1) return null;

  const tradeRate = config?.buildings["TradePost"]?.find(
    (l) => l.level === tradePost.level,
  )?.tradeRate ?? 1;

  return (
    <>
      <Button
        type="button"
        onClick={() => setOpen(true)}
        className="mx-4 mb-2 mt-2 cursor-pointer rounded border border-amber-700 bg-amber-950/30 px-3 py-1 text-xs font-medium text-amber-400 transition-colors hover:bg-amber-900/50"
      >
        Trade
      </Button>

      <Modal open={open} onClose={() => setOpen(false)}>
        <TradePanel
          resources={village.resources}
          tradeRate={tradeRate}
          tradePostLevel={tradePost.level}
          mutation={mutation}
          onClose={() => setOpen(false)}
        />
      </Modal>
    </>
  );
}
