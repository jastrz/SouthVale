import { useRef, useState } from "react";
import { useVillage, useTrade, useGameConfig } from "../../api/hooks/useQueries";
import { TradePanel } from "./TradePanel";

export function TradeController({ villageId }: { villageId: string }) {
  const { data: village } = useVillage(villageId);
  const { data: config } = useGameConfig();
  const mutation = useTrade(villageId);
  const dialogRef = useRef<HTMLDialogElement>(null);
  const [openCount, setOpenCount] = useState(0);

  if (!village) return null;

  const tradePost = village.buildings.find((b) => b.type === "TradePost");
  if (!tradePost || tradePost.level < 1) return null;

  const tradeRate = config?.buildings["TradePost"]?.find(
    (l) => l.level === tradePost.level,
  )?.tradeRate ?? 1;

  return (
    <>
      <button
        type="button"
        onClick={() => {
          setOpenCount((c) => c + 1);
          dialogRef.current?.showModal();
        }}
        className="mx-4 mb-2 mt-2 cursor-pointer rounded border border-amber-700 bg-amber-950/30 px-3 py-1 text-xs font-medium text-amber-400 transition-colors hover:bg-amber-900/50"
      >
        Trade
      </button>

      <dialog
        ref={dialogRef}
        className="w-full max-w-md rounded-xl bg-slate-800 p-0 text-white backdrop:bg-black/20 shadow-2xl"
        style={{ margin: "auto" }}
      >
        <TradePanel
          key={openCount}
          resources={village.resources}
          tradeRate={tradeRate}
          tradePostLevel={tradePost.level}
          mutation={mutation}
          onClose={() => dialogRef.current?.close()}
        />
      </dialog>
    </>
  );
}
