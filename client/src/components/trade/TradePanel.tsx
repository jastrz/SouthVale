import { useState } from "react";
import type { UseMutationResult } from "@tanstack/react-query";
import type { TradeRequest, ResourceType, ResourcesDto } from "../../api/types";
import { Icon } from "../Icon";
import { NumberInput } from "../NumberInput";
import { RESOURCE_ICONS } from "../../lib/helpers";

const RESOURCES: { type: ResourceType; label: string }[] = [
  { type: "Wood", label: "Wood" },
  { type: "Clay", label: "Clay" },
  { type: "Iron", label: "Iron" },
  { type: "Beer", label: "Beer" },
];

export function TradePanel({
  resources,
  tradeRate,
  tradePostLevel,
  mutation,
  onClose,
}: {
  resources: ResourcesDto;
  tradeRate: number;
  tradePostLevel: number;
  mutation: UseMutationResult<unknown, unknown, TradeRequest, unknown>;
  onClose: () => void;
}) {
  const [giveType, setGiveType] = useState<ResourceType>("Wood");
  const [getType, setGetType] = useState<ResourceType>("Clay");
  const [giveAmount, setGiveAmount] = useState(0);

  const receiveAmount = Math.floor(giveAmount * tradeRate);
  const giveMax = resources[giveType.toLowerCase() as keyof ResourcesDto] as number;
  const canTrade = giveAmount > 0 && giveAmount <= giveMax && receiveAmount >= 1 && giveType !== getType;

  const handleTrade = () => {
    if (!canTrade) return;
      mutation.mutate(
      { giveType, giveAmount, receiveType: getType },
      { onSuccess: onClose },
    );
  };

  return (
    <section className="flex flex-col gap-3 border-t border-amber-900/60 bg-amber-950/20 px-4 py-4">
      <div className="flex items-center justify-between">
        <h3 className="text-xs font-bold tracking-widest text-amber-400 uppercase">
          Trade Post (Lv.{tradePostLevel})
        </h3>
        <span className="text-[10px] text-amber-500">
          Rate: {(tradeRate * 100).toFixed(0)}%
        </span>
        <button
          type="button"
          onClick={onClose}
          className="cursor-pointer text-[11px] text-slate-500 hover:text-slate-300"
        >
          Close
        </button>
      </div>

      <div className="grid grid-cols-[1fr_auto_1fr] items-end gap-2">
        <div>
          <label className="mb-0.5 block text-[9px] text-slate-400 uppercase tracking-wider">Give</label>
          <select
            value={giveType}
            onChange={(e) => {
              const t = e.target.value as ResourceType;
              setGiveType(t);
              if (t === getType) setGetType(RESOURCES.find((r) => r.type !== t)!.type);
            }}
            className="mb-1 w-full rounded border border-slate-600 bg-slate-900 px-1.5 py-1 text-xs text-white outline-none focus:border-amber-500"
          >
            {RESOURCES.map((r) => (
              <option key={r.type} value={r.type}>{r.label}</option>
            ))}
          </select>
          <NumberInput value={giveAmount} onChange={setGiveAmount} max={giveMax} />
        </div>

        <div className="flex items-center pb-3 text-lg text-amber-400">→</div>

        <div>
          <label className="mb-0.5 block text-[9px] text-slate-400 uppercase tracking-wider">Get</label>
          <select
            value={getType}
            onChange={(e) => {
              const t = e.target.value as ResourceType;
              setGetType(t);
              if (t === giveType) setGiveType(RESOURCES.find((r) => r.type !== t)!.type);
            }}
            className="mb-1 w-full rounded border border-slate-600 bg-slate-900 px-1.5 py-1 text-xs text-white outline-none focus:border-amber-500"
          >
            {RESOURCES.map((r) => (
              <option key={r.type} value={r.type}>{r.label}</option>
            ))}
          </select>
          <div className="flex h-58px items-center justify-center rounded border border-slate-700 bg-slate-800/40 text-sm text-green-400">
            {giveAmount > 0 ? (
              <span className="flex items-center gap-1">
                <Icon src={RESOURCE_ICONS[getType.toLowerCase()]} size={14} />
                {receiveAmount}
              </span>
            ) : (
              <span className="text-[10px] text-slate-500">Enter amount</span>
            )}
          </div>
        </div>
      </div>

      {giveType === getType && (
        <p className="text-[10px] text-red-400">Select different resources</p>
      )}
      {giveAmount > giveMax && (
        <p className="text-[10px] text-red-400">
          Not enough {giveType.toLowerCase()}
        </p>
      )}

      <button
        type="button"
        onClick={handleTrade}
        disabled={!canTrade || mutation.isPending}
        className="w-full cursor-pointer rounded bg-amber-700 px-3 py-1.5 text-xs font-medium text-white transition-colors hover:bg-amber-600 disabled:cursor-not-allowed disabled:opacity-40 disabled:hover:bg-amber-700"
      >
        {mutation.isPending ? "Trading..." : "Trade"}
      </button>
    </section>
  );
}
