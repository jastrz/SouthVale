import { useRef, useState } from "react";
import {
  useGameStateStore,
  selectActiveVillage,
} from "../store/gameStateStore";
import {
  useVillage,
  useGameConfig,
  useRenameVillage,
} from "../api/hooks/useQueries";
import { Icon } from "./Icon";
import { RESOURCE_ICONS } from "../lib/helpers";
import { LoginBar } from "./LoginBar";

export function TopBar() {
  const activeVillageId = useGameStateStore((s) => s.activeVillageId);
  const storeVillage = useGameStateStore(selectActiveVillage);

  return (
    <div className="absolute left-0 right-0 top-0 z-10 flex justify-center">
      <div className="flex items-center gap-4 rounded-b-lg border border-t-0 border-slate-800 bg-slate-950/70 px-4 py-2 text-xs shadow-lg">
        <LoginBar />

        {activeVillageId && storeVillage && (
          <>
            <div className="h-4 w-px bg-slate-700" />
            <VillageContent villageId={activeVillageId} />
          </>
        )}
      </div>
    </div>
  );
}

function VillageContent({ villageId }: { villageId: string }) {
  const { data: village } = useVillage(villageId);
  const { data: gameConfig } = useGameConfig();
  const renameMutation = useRenameVillage(villageId);
  const [editing, setEditing] = useState(false);
  const [draft, setDraft] = useState("");
  const inputRef = useRef<HTMLInputElement>(null);

  if (!village) return <div className="h-5 w-32 animate-pulse rounded bg-slate-800" />;

  const warehouseLevel =
    village.buildings.find((b) => b.type === "Warehouse")?.level ?? 0;
  const granaryLevel =
    village.buildings.find((b) => b.type === "Granary")?.level ?? 0;
  const warehouseCapacity = gameConfig?.buildings["Warehouse"]?.find(
    (l) => l.level === warehouseLevel,
  )?.warehouseCapacity;
  const granaryCapacity = gameConfig?.buildings["Granary"]?.find(
    (l) => l.level === granaryLevel,
  )?.granaryCapacity;

  const resources = [
    {
      value: Math.floor(village.resources.wood),
      max: warehouseCapacity,
      icon: RESOURCE_ICONS.wood,
    },
    {
      value: Math.floor(village.resources.clay),
      max: warehouseCapacity,
      icon: RESOURCE_ICONS.clay,
    },
    {
      value: Math.floor(village.resources.iron),
      max: warehouseCapacity,
      icon: RESOURCE_ICONS.iron,
    },
    {
      value: Math.floor(village.resources.crop),
      max: granaryCapacity,
      icon: RESOURCE_ICONS.crop,
    },
  ];

  const startEditing = () => {
    setDraft(village.name);
    setEditing(true);
    requestAnimationFrame(() => {
      inputRef.current?.focus();
      inputRef.current?.select();
    });
  };

  const submit = () => {
    const trimmed = draft.trim();
    if (trimmed && trimmed !== village.name) renameMutation.mutate(trimmed);
    setEditing(false);
  };

  return (
    <>
      <div className="shrink-0">
        {editing ? (
          <input
            ref={inputRef}
            className="bg-slate-700 px-1 text-sm font-bold text-white outline-none ring-1 ring-slate-500"
            value={draft}
            onChange={(e) => setDraft(e.target.value)}
            onBlur={submit}
            onKeyDown={(e) => {
              if (e.key === "Enter") submit();
              if (e.key === "Escape") setEditing(false);
            }}
          />
        ) : (
          <h2
            className="cursor-pointer text-sm font-bold text-white hover:text-slate-300"
            onClick={startEditing}
            title="Click to rename"
          >
            {village.name}{" "}
            <span className="font-normal text-slate-400">
              ({village.coordinates.x}, {village.coordinates.y})
            </span>
          </h2>
        )}
      </div>
      <div className="flex gap-4">
        {resources.map((r) => (
          <div key={r.icon} className="flex items-center gap-1 text-slate-300">
            <Icon src={r.icon} size={24} />
            <span>{r.value.toLocaleString()}</span>
            {r.max !== undefined && (
              <span className="text-slate-500">/{r.max.toLocaleString()}</span>
            )}
          </div>
        ))}
      </div>
    </>
  );
}
