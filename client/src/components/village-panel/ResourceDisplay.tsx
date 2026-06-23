import { Icon } from "../Icon";

function ResourceItem({
  label,
  value,
  icon,
}: {
  label: string;
  value: number;
  icon: string;
}) {
  return (
    <div className="flex items-center justify-between rounded bg-slate-800/40 px-2.5 py-1.5 text-xs">
      <span className="flex items-center gap-1.5 text-slate-400">
        <Icon src={icon} size={32} />
        {label}
      </span>
      <span className="font-medium text-white">{value.toLocaleString()}</span>
    </div>
  );
}

export function ResourceDisplay({
  wood,
  clay,
  iron,
  crop,
}: {
  wood: number;
  clay: number;
  iron: number;
  crop: number;
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
          icon="/icons/resources/wood.png"
        />
        <ResourceItem
          label="Clay"
          value={clay}
          icon="/icons/resources/clay.png"
        />
        <ResourceItem
          label="Iron"
          value={iron}
          icon="/icons/resources/iron.png"
        />
        <ResourceItem
          label="Crop"
          value={crop}
          icon="/icons/resources/crop.png"
        />
      </div>
    </section>
  );
}
