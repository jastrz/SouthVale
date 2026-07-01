import type { VillageListItemDto, VillageStatusDto } from "../../api/types";
import { Icon } from "../Icon";
import { RESOURCE_ICONS, TROOP_ICONS } from "../../lib/helpers";

export function VillageListItem({
  village,
  status,
  isActive,
  incomingCount = 0,
  movementCount = 0,
  onClick,
}: {
  village: VillageListItemDto;
  status?: VillageStatusDto;
  isActive: boolean;
  incomingCount?: number;
  movementCount?: number;
  onClick: () => void;
}) {
  const bc = status?.buildOrderCount ?? 0;
  const tc = status?.trainOrderCount ?? 0;
  const r = status?.resources ?? village.resources;
  const t = status?.troops ?? village.troops;

  return (
    <li>
      <button
        type="button"
        onClick={onClick}
        aria-current={isActive ? "true" : undefined}
        className={[
          "w-full cursor-pointer border-none px-4 py-1 text-left text-sm transition-colors rounded-sm",
          isActive
            ? "bg-blue-600 text-white"
            : "bg-transparent text-white hover:bg-slate-800",
        ].join(" ")}
      >
        <div className="flex items-center gap-2">
          <span className="font-semibold">{village.name}</span>
          {(bc > 0 || tc > 0 || movementCount > 0 || incomingCount > 0) && (
            <span className="flex gap-1 text-[10px] text-amber-400">
              {incomingCount > 0 && (
                <span className="text-red-400">A:{incomingCount}</span>
              )}
              {bc > 0 && <span>B:{bc}</span>}
              {tc > 0 && <span>T:{tc}</span>}
              {movementCount > 0 && <span>M:{movementCount}</span>}
            </span>
          )}
        </div>
        <div
          className={[
            "mt-0.5 flex gap-1.5 text-[11px]",
            isActive ? "text-blue-100" : "text-slate-400",
          ].join(" ")}
        >
          <span className="flex items-center gap-0.5">
            <Icon src={RESOURCE_ICONS.wood} size={10} />
            {r.wood}
          </span>
          <span className="flex items-center gap-0.5">
            <Icon src={RESOURCE_ICONS.clay} size={10} />
            {r.clay}
          </span>
          <span className="flex items-center gap-0.5">
            <Icon src={RESOURCE_ICONS.iron} size={10} />
            {r.iron}
          </span>
          <span className="flex items-center gap-0.5">
            <Icon src={RESOURCE_ICONS.crop} size={10} />
            {r.crop}
          </span>
        </div>
        <div
          className={[
            "flex gap-1.5 text-[11px]",
            isActive ? "text-blue-100" : "text-slate-400",
          ].join(" ")}
        >
          <span className="flex items-center gap-0.5">
            <Icon src={TROOP_ICONS.Swordsman} size={10} />
            {t.swordsmen}
          </span>
          <span className="flex items-center gap-0.5">
            <Icon src={TROOP_ICONS.Archer} size={10} />
            {t.archers}
          </span>
          <span className="flex items-center gap-0.5">
            <Icon src={TROOP_ICONS.Settler} size={10} />
            {t.settlers}
          </span>
        </div>
      </button>
    </li>
  );
}
