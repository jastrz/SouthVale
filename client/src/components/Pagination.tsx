import { useState } from "react";

export function Pagination({
  page,
  totalPages,
  onChange,
}: {
  page: number;
  totalPages: number;
  onChange: (p: number) => void;
}) {
  const [editValue, setEditValue] = useState<string | null>(null);
  const display = editValue ?? String(page);
  const btn =
    "rounded bg-slate-700 px-2 py-0.5 text-xs text-slate-300 hover:bg-slate-600 disabled:opacity-40";
  const submit = () => {
    if (editValue === null) return;
    const p = Number(editValue);
    setEditValue(null);
    if (p >= 1 && p <= totalPages) onChange(p);
  };

  return (
    <div className="flex items-center justify-center gap-2">
      <button
        className={btn}
        onClick={() => onChange(page - 1)}
        disabled={page <= 1}
      >
        Prev
      </button>
      <span className="rounded bg-slate-700 px-2 py-0.5 text-xs text-slate-300">
        <input
          className="bg-transparent text-xs text-slate-300 text-center mr-1"
          style={{ width: `${String(totalPages).length}ch` }}
          value={display}
          onChange={(e) => setEditValue(e.target.value)}
          onBlur={submit}
          onKeyDown={(e) => e.key === "Enter" && submit()}
        />
        / {totalPages}
      </span>
      <button
        className={btn}
        onClick={() => onChange(page + 1)}
        disabled={page >= totalPages}
      >
        Next
      </button>
    </div>
  );
}
