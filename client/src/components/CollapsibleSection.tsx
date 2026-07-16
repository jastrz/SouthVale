export function CollapsibleSection({
  label,
  open,
  onToggle,
  className = "text-slate-600 hover:text-slate-900",
  children,
}: {
  label: string;
  open: boolean;
  onToggle: () => void;
  className?: string;
  children: React.ReactNode;
}) {
  return (
    <section>
      <button
        onClick={onToggle}
        className={`mb-2 flex w-full items-center gap-1 font-bold tracking-widest uppercase transition-colors ${className}`}
      >
        <span className="text-[10px]">{open ? "▾" : "▸"}</span>
        {label}
      </button>
      {open && children}
    </section>
  );
}
