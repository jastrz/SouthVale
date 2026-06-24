import type { ResourcesDto } from "../../api/types";
import { Icon } from "../Icon";
import { RESOURCE_ICONS } from "./helpers";

const RESOURCE_ROWS: { key: keyof ResourcesDto; label: string; icon: string }[] = [
  { key: "wood", label: "Wood", icon: RESOURCE_ICONS.wood },
  { key: "clay", label: "Clay", icon: RESOURCE_ICONS.clay },
  { key: "iron", label: "Iron", icon: RESOURCE_ICONS.iron },
  { key: "crop", label: "Crop", icon: RESOURCE_ICONS.crop },
];

export function ResourceCost({ value }: { value: ResourcesDto }) {
  return (
    <div className="grid grid-cols-2 gap-x-3 gap-y-0.5">
      {RESOURCE_ROWS.map(
        ({ key, label, icon }) =>
          value[key] > 0 && (
            <span key={key} className="flex items-center gap-1 text-slate-400">
              <Icon src={icon} size={12} /> {label} <span className="text-white">{value[key]}</span>
            </span>
          ),
      )}
    </div>
  );
}
