import { Icon } from "../Icon";
import { RESOURCE_ICONS } from "./helpers";

function ResourceItem({
  label,
  value,
  max,
  icon,
}: {
  label: string;
  value: number;
  max?: number;
  icon: string;
}) {
  return (
    <div className="rounded bg-slate-800/40 px-2.5 py-1.5 text-xs text-center">
      <div className="flex items-center justify-center gap-1.5 text-slate-400">
        <Icon src={icon} size={16} />
        {label}
      </div>
      <div className="font-small text-white">
        {value.toLocaleString()}
        {max !== undefined && (
          <span className="text-slate-500"> / {max.toLocaleString()}</span>
        )}
      </div>
    </div>
  );
}

export function ResourceDisplay({
  wood,
  clay,
  iron,
  crop,
  warehouseCapacity,
  granaryCapacity,
}: {
  wood: number;
  clay: number;
  iron: number;
  crop: number;
  warehouseCapacity?: number;
  granaryCapacity?: number;
}) {
  return (
    <section className="border-b border-slate-800 px-4 py-3">
      <h3 className="mb-2 text-xs font-bold tracking-widest text-slate-400 uppercase">
        Resources
      </h3>
      <div className="grid grid-cols-2 gap-2">
        <ResourceItem
          label="Wood"
          value={wood}
          max={warehouseCapacity}
          icon={RESOURCE_ICONS.wood}
        />
        <ResourceItem
          label="Clay"
          value={clay}
          max={warehouseCapacity}
          icon={RESOURCE_ICONS.clay}
        />
        <ResourceItem
          label="Iron"
          value={iron}
          max={warehouseCapacity}
          icon={RESOURCE_ICONS.iron}
        />
        <ResourceItem
          label="Crop"
          value={crop}
          max={granaryCapacity}
          icon={RESOURCE_ICONS.crop}
        />
      </div>
    </section>
  );
}
