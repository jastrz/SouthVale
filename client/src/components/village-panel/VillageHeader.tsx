export function VillageHeader({
  name,
  x,
  y,
}: {
  name: string;
  x: number;
  y: number;
}) {
  return (
    <div className="border-b border-slate-800 px-4 py-3">
      <h2 className="text-sm font-bold text-white">{name}</h2>
      <p className="text-xs text-slate-400">
        {x}, {y}
      </p>
    </div>
  );
}
