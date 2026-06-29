export function Pagination({
  page,
  totalPages,
  onChange,
}: {
  page: number;
  totalPages: number;
  onChange: (p: number) => void;
}) {
  const btn =
    "rounded bg-slate-700 px-2 py-0.5 text-xs text-slate-300 hover:bg-slate-600 disabled:opacity-40";
  return (
    <div className="flex items-center justify-center gap-2">
      <button
        className={btn}
        onClick={() => onChange(page - 1)}
        disabled={page <= 1}
      >
        Prev
      </button>
      <span className="text-xs text-slate-400">
        {page} / {totalPages}
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
