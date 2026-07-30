import { useState, type CSSProperties } from "react";
import { useGameStateStore } from "../store/gameStateStore";
import type { MapVillage } from "../api/types";
import { useAttack, useVillage, useGameConfig } from "../api/hooks/useQueries";
import { Modal } from "./Modal";
import { NumberInput } from "./NumberInput";
import { Icon } from "./Icon";
import { travelTime, TROOP_ICONS } from "../lib/helpers";

function Eta({ from, to, speed, multiplier }: { from: { x: number; y: number }; to: { x: number; y: number }; speed: number; multiplier: number }) {
  return <div className="text-yellow-400">time: {travelTime(from.x, from.y, to.x, to.y, speed, multiplier)}</div>;
}

function EnemyAttackDialog({
  village,
  onClose,
}: {
  village: MapVillage & { kind: "enemy" };
  onClose: () => void;
}) {
  const { data: activeVillage } = useVillage(useGameStateStore.getState().activeVillageId ?? "");
  const { data: gameConfig } = useGameConfig();
  const mutation = useAttack(activeVillage?.id ?? "");
  const [swordsmen, setSwordsmen] = useState(0);
  const [archers, setArchers] = useState(0);
  const [dogs, setDogs] = useState(0);
  const [horsemen, setHorsemen] = useState(0);
  const [llamaRiders, setLlamaRiders] = useState(0);
  const ICON_SIZE = 18;

  const troops = activeVillage?.troops;
  const selected = [
    { type: "Swordsman" as const, count: swordsmen },
    { type: "Archer" as const, count: archers },
    { type: "Dogs" as const, count: dogs },
    { type: "Horsemen" as const, count: horsemen },
    { type: "LlamaRiders" as const, count: llamaRiders },
  ].filter((t) => t.count > 0);

  if (!activeVillage || !troops || !gameConfig) return null;

  const getSpeed = (type: string) => gameConfig.troops[type]?.speed ?? 5;
  const speed = selected.length > 0
    ? Math.min(...selected.map((t) => getSpeed(t.type)))
    : Math.min(...["Swordsman","Archer","Dogs","Horsemen","LlamaRiders"].map(getSpeed));
  const mult = gameConfig.travelSpeedMultiplier;

  const handleAttack = () => {
    if (selected.length === 0 || !activeVillage) return;
    mutation.mutate(
      { troops: selected.map((t) => ({ troopType: t.type, count: t.count })), targetVillageId: village.id },
      { onSuccess: () => { setSwordsmen(0); setArchers(0); setDogs(0); setHorsemen(0); setLlamaRiders(0); onClose(); } },
    );
  };

  return (
    <div className="flex flex-col gap-3 p-4">
      <div className="flex items-center justify-between">
        <h3 className="text-xs font-bold tracking-widest text-red-400 uppercase">Prepare Attack</h3>
        <button type="button" onClick={onClose} className="cursor-pointer text-xs text-slate-500 hover:text-slate-300">Cancel</button>
      </div>
      <div className="rounded bg-slate-800/40 px-2.5 py-1.5 text-xs">
        <div className="font-medium text-white">{village.name}</div>
        <div className="text-slate-400">({village.coordinates.x}, {village.coordinates.y}) · Population: {village.population} · <Eta from={activeVillage.coordinates} to={village.coordinates} speed={speed} multiplier={mult} /></div>
      </div>
      <div className="grid grid-cols-3 gap-1">
        {troops.swordsmen > 0 && <NumberInput label={<><Icon src={TROOP_ICONS.Swordsman} size={ICON_SIZE} /> Swordsmen</>} value={swordsmen} onChange={setSwordsmen} max={troops.swordsmen} />}
        {troops.archers > 0 && <NumberInput label={<><Icon src={TROOP_ICONS.Archer} size={ICON_SIZE} /> Archers</>} value={archers} onChange={setArchers} max={troops.archers} />}
        {troops.dogs > 0 && <NumberInput label={<><Icon src={TROOP_ICONS.Dogs} size={ICON_SIZE} /> Dogs</>} value={dogs} onChange={setDogs} max={troops.dogs} />}
        {troops.horsemen > 0 && <NumberInput label={<><Icon src={TROOP_ICONS.Horsemen} size={ICON_SIZE} /> Horsemen</>} value={horsemen} onChange={setHorsemen} max={troops.horsemen} />}
        {troops.llamaRiders > 0 && <NumberInput label={<><Icon src={TROOP_ICONS.LlamaRiders} size={ICON_SIZE} /> LlamaRiders</>} value={llamaRiders} onChange={setLlamaRiders} max={troops.llamaRiders} />}
      </div>
      <button
        type="button"
        onClick={handleAttack}
        disabled={selected.length === 0 || mutation.isPending}
        className="w-full cursor-pointer rounded bg-red-700 px-3 py-1.5 text-xs font-medium text-white transition-colors hover:bg-red-600 disabled:cursor-not-allowed disabled:opacity-40"
      >
        {mutation.isPending ? "Sending…" : "Send Attack"}
      </button>
    </div>
  );
}

export function VillagePopup() {
  const targetVillage = useGameStateStore((s) => s.targetVillage);
  const setTargetVillage = useGameStateStore((s) => s.setTargetVillage);
  const targetVillagePos = useGameStateStore((s) => s.targetVillagePos);
  const activeVillageId = useGameStateStore((s) => s.activeVillageId);
  const { data: activeVillage } = useVillage(activeVillageId ?? "");
  const [showAttack, setShowAttack] = useState(false);

  const handleClose = () => {
    setShowAttack(false);
    setTargetVillage(null);
  };

  if (!targetVillage || targetVillage.kind !== "enemy") return null;

  const isMobile = window.innerWidth < 768;
  const pos = targetVillagePos;
  const above = pos ? pos.y > window.innerHeight / 2 : true;
  const canAttack = activeVillage && (activeVillage.troops.swordsmen + activeVillage.troops.archers + activeVillage.troops.dogs + activeVillage.troops.horsemen + activeVillage.troops.llamaRiders) > 0;
  const dialogStyle: CSSProperties = isMobile || !pos
    ? { margin: "auto" }
    : { margin: 0, position: "fixed", left: Math.min(pos.x + 14, window.innerWidth - 360), top: above ? pos.y - 14 : pos.y + 14 };

  return (
    <Modal open={!!targetVillage} onClose={handleClose} style={dialogStyle}>
      {!showAttack ? (
        <div className="flex flex-col gap-3 p-4">
          <div className="flex items-center justify-between">
            <h2 className="text-sm font-bold text-white">{targetVillage.name}</h2>
            <button type="button" onClick={handleClose} className="cursor-pointer text-xs text-slate-500 hover:text-slate-300">✕</button>
          </div>
          <div className="text-[11px] tracking-wider text-slate-400 uppercase">
            Enemy Village · ({targetVillage.coordinates.x}, {targetVillage.coordinates.y})
          </div>
          <div className="grid grid-cols-2 gap-x-3 gap-y-0.5 text-xs">
            <span className="text-slate-400">Population</span><span className="text-right">{targetVillage.population}</span>
            <span className="text-slate-400">Owner</span><span className="truncate text-right text-slate-300">{targetVillage.playerName}</span>
          </div>
          {activeVillage && canAttack ? (
            <button
              type="button"
              onClick={() => setShowAttack(true)}
              className="w-full cursor-pointer rounded border border-red-700 bg-red-950/30 px-3 py-2 text-xs font-medium text-red-400 transition-colors hover:bg-red-900/50"
            >
              Attack
            </button>
          ) : activeVillage ? (
            <button
              type="button"
              disabled
              className="w-full cursor-not-allowed rounded border border-slate-700 bg-slate-900/30 px-3 py-2 text-xs font-medium text-slate-600"
            >
              No troops available
            </button>
          ) : null}
        </div>
      ) : (
        <EnemyAttackDialog village={targetVillage as MapVillage & { kind: "enemy" }} onClose={handleClose} />
      )}
    </Modal>
  );
}
