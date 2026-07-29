import { useMemo, useRef, useState } from "react";
import {
  useGameStateStore,
  selectActiveVillage,
} from "../store/gameStateStore";
import {
  useVillage,
  useGameConfig,
  useRenameVillage,
  useReports,
} from "../api/hooks/useQueries";
import { Icon } from "./Icon";
import { RESOURCE_ICONS } from "../lib/helpers";
import { LoginBar } from "./LoginBar";

export function TopBar() {
  const activeVillageId = useGameStateStore((s) => s.activeVillageId);
  const storeVillage = useGameStateStore(selectActiveVillage);
  const currentView = useGameStateStore((s) => s.currentView);
  const setCurrentView = useGameStateStore((s) => s.setCurrentView);
  const { data: reportsData } = useReports();
  const unreadCount = reportsData?.unreadCount ?? 0;

  return (
    <div className="pointer-events-none absolute left-0 right-0 top-0 z-20 flex flex-col items-center gap-1">
      <div
        className="pointer-events-auto grid w-full items-center gap-2 rounded-b-lg border border-t-0 border-slate-800 bg-slate-950/70 px-4 py-1 text-xs shadow-lg sm:w-auto sm:flex sm:flex-wrap sm:gap-4"
        style={{
          gridTemplateAreas: '"name login" "resources resources"',
          gridTemplateColumns: "1fr auto",
        }}
      >
        {activeVillageId && storeVillage && (
          <VillageContent villageId={activeVillageId} />
        )}
        <div className="[grid-area:login] sm:ml-auto">
          <LoginBar />
        </div>
      </div>

      <nav className="pointer-events-auto flex gap-1 rounded-lg border border-slate-800 bg-slate-950/70 px-2 py-0.5 flex-wrap justify-center sm:flex-nowrap">
        <Tab
          active={currentView === "map"}
          onClick={() => setCurrentView("map")}
        >
          Map
        </Tab>
        <Tab
          active={currentView === "notifications"}
          onClick={() => setCurrentView("notifications")}
          badge={unreadCount}
        >
          Notifications
        </Tab>
        <Tab
          active={currentView === "leaderboard"}
          onClick={() => setCurrentView("leaderboard")}
        >
          Leaderboard
        </Tab>
        <Tab
          active={currentView === "gameinfo"}
          onClick={() => setCurrentView("gameinfo")}
        >
          Game Info
        </Tab>
      </nav>
    </div>
  );
}

function Tab({
  active,
  onClick,
  badge,
  children,
}: {
  active: boolean;
  onClick: () => void;
  badge?: number;
  children: React.ReactNode;
}) {
  return (
    <button
      className={`relative rounded px-1.5 py-0.5 text-[10px] font-medium tracking-wide uppercase transition-colors sm:px-2.5 sm:py-1 sm:text-xs ${
        active
          ? "bg-slate-700 text-white"
          : "text-slate-400 hover:bg-slate-800 hover:text-slate-200"
      }`}
      onClick={onClick}
    >
      {children}
      {badge !== undefined && badge > 0 && (
        <span className="absolute -top-1 -right-1 flex h-3.5 min-w-3.5 items-center justify-center rounded-full bg-red-600 px-0.5 text-[8px] font-bold text-white sm:-top-1.5 sm:-right-1.5 sm:h-4 sm:min-w-4 sm:px-1 sm:text-[10px]">
          {badge > 99 ? "99+" : badge}
        </span>
      )}
    </button>
  );
}

function VillageContent({ villageId }: { villageId: string }) {
  const { data: village } = useVillage(villageId);
  const { data: gameConfig } = useGameConfig();
  const renameMutation = useRenameVillage(villageId);
  const [editing, setEditing] = useState(false);
  const [draft, setDraft] = useState("");
  const inputRef = useRef<HTMLInputElement>(null);

  const production = useMemo(() => {
    if (!village || !gameConfig) return null;
    const total = { wood: 0, clay: 0, iron: 0, beer: 0 };
    for (const b of village.buildings) {
      const cfg = gameConfig.buildings[b.type]?.find((l) => l.level === b.level)?.productionPerHour;
      if (cfg) {
        total.wood += cfg.wood;
        total.clay += cfg.clay;
        total.iron += cfg.iron;
        total.beer += cfg.beer;
      }
    }
    return total;
  }, [village, gameConfig]);

  const upkeep = useMemo(() => {
    if (!village || !gameConfig) return 0;
    const fields: { type: string; key: keyof typeof village.troops }[] = [
      { type: "Swordsman", key: "swordsmen" },
      { type: "Archer", key: "archers" },
      { type: "Settler", key: "settlers" },
      { type: "Dogs", key: "dogs" },
      { type: "Horsemen", key: "horsemen" },
      { type: "LlamaRiders", key: "llamaRiders" },
    ];
    let beer = 0;
    for (const f of fields) {
      const cfg = gameConfig.troops[f.type];
      if (cfg) beer += cfg.upkeep * village.troops[f.key];
    }
    return beer;
  }, [village, gameConfig]);

  if (!village)
    return <div className="h-5 w-32 animate-pulse rounded bg-slate-800" />;

  const warehouseLevel =
    village.buildings.find((b) => b.type === "Warehouse")?.level ?? 0;
  const warehouseCapacity = gameConfig?.buildings["Warehouse"]?.find(
    (l) => l.level === warehouseLevel,
  )?.warehouseCapacity;

  const resources = [
    {
      key: "wood" as const,
      value: Math.floor(village.resources.wood),
      max: warehouseCapacity,
      icon: RESOURCE_ICONS.wood,
    },
    {
      key: "clay" as const,
      value: Math.floor(village.resources.clay),
      max: warehouseCapacity,
      icon: RESOURCE_ICONS.clay,
    },
    {
      key: "iron" as const,
      value: Math.floor(village.resources.iron),
      max: warehouseCapacity,
      icon: RESOURCE_ICONS.iron,
    },
    {
      key: "beer" as const,
      value: Math.floor(village.resources.beer),
      max: warehouseCapacity,
      icon: RESOURCE_ICONS.beer,
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
    <div className="contents">
      <div className="[grid-area:name] shrink-0">
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
            className="cursor-pointer text-xs font-bold text-white hover:text-slate-300 sm:text-sm"
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
      <div className="[grid-area:resources] flex justify-center gap-1 sm:gap-4">
        {resources.map((r) => (
          <div key={r.icon} className="flex items-center gap-1 text-slate-300">
            <Icon src={r.icon} size={24} />
            <div className="flex flex-col">
              <div>
                <span>{r.value.toLocaleString()}</span>
                {r.max !== undefined && (
                  <span className="hidden text-slate-500 sm:inline">/{r.max.toLocaleString()}</span>
                )}
              </div>
              {production && (
                <div className="text-[10px] leading-tight">
                  <span className="text-green-400">+{Math.floor(production[r.key])}/h</span>
                  {r.key === "beer" && upkeep > 0 && (
                    <span className="text-red-400"> -{upkeep.toFixed(1)}/h</span>
                  )}
                </div>
              )}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
