export function CollapsibleSection({
  label,
  open,
  onToggle,
  className = "text-slate-600 hover:text-slate-900",
  children,
}: {
  label: React.ReactNode;
  open: boolean;
  onToggle: () => void;
  className?: string;
  children: React.ReactNode;
}) {
  return (
    <section>
      <button
        onClick={onToggle}
        className={`mb-2 flex cursor-pointer items-center gap-1 font-bold tracking-widest transition-colors ${className}`}
      >
        <span className="text-sm">{open ? "▾" : "▸"}</span>
        {label}
      </button>
      <div
        className={`overflow-hidden transition-all duration-150 ease-in-out ${
          open ? "max-h-none opacity-100 pb-3" : "max-h-0 opacity-0"
        }`}
      >
        {children}
      </div>
    </section>
  );
}
