export function NumberInput({
  value,
  onChange,
  max,
}: {
  value: number;
  onChange: (v: number) => void;
  max: number;
}) {
  return (
    <div>
      <input
        type="number"
        min={0}
        max={max}
        value={value || ""}
        onChange={(e) =>
          onChange(Math.max(0, Number.parseInt(e.target.value) || 0))
        }
        className="w-full rounded border border-slate-600 bg-slate-900 px-1.5 py-1 text-xs text-white outline-none focus:border-blue-500"
        placeholder="0"
      />
      <button
        type="button"
        onClick={() => onChange(max)}
        className="mt-0.5 w-full cursor-pointer rounded bg-slate-700/60 px-1 py-0.5 text-[9px] text-slate-400 transition-colors hover:bg-slate-600/60 hover:text-slate-200"
      >
        Max: {max}
      </button>
    </div>
  );
}
