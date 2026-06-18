function ResourceItem({ label, value }: { label: string; value: number }) {
  return (
    <div className="flex items-center justify-between rounded bg-slate-800/40 px-2.5 py-1.5 text-xs">
      <span className="text-slate-400">{label}</span>
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
        <ResourceItem label="Wood" value={wood} />
        <ResourceItem label="Clay" value={clay} />
        <ResourceItem label="Iron" value={iron} />
        <ResourceItem label="Crop" value={crop} />
      </div>
    </section>
  );
}
