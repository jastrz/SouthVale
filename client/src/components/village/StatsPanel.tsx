import type { GameConfigDto } from "../../api/types";

export function StatsPanel({
  buildings,
  config,
  infantryAttack,
  cavalryAttack,
}: {
  buildings: readonly { type: string; level: number }[];
  config?: GameConfigDto;
  infantryAttack: number;
  cavalryAttack: number;
}) {
  let defense = 1;

  for (const b of buildings) {
    const lvl = config?.buildings[b.type]?.find((l) => l.level === b.level);
    if (!lvl) continue;
    defense *= lvl.defenseMultiplier;
  }

  return (
    <div className="space-y-1 px-4 py-3 text-xs">
      <div className="text-slate-300">
        Infantry Attack: <span className="text-slate-100">{infantryAttack.toFixed(2)}x</span>{" "}
        <span className="text-slate-300">(Empire-wide)</span>
      </div>
      <div className="text-slate-300">
        Cavalry Attack: <span className="text-slate-100">{cavalryAttack.toFixed(2)}x</span>{" "}
        <span className="text-slate-300">(Empire-wide)</span>
      </div>
      <div className="text-slate-300">
        Defense: <span className="text-slate-100">{defense.toFixed(2)}x</span>
      </div>
    </div>
  );
}
