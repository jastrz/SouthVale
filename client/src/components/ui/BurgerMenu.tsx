import { type ReactNode, useState, useRef, useEffect } from "react";

type BurgerMenuItem = {
  label: string;
  onClick: () => void;
  danger?: boolean;
  icon?: ReactNode;
};

type BurgerMenuProps = {
  items: BurgerMenuItem[];
};

export function BurgerMenu({ items }: BurgerMenuProps) {
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!open) return;
    const close = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false);
    };
    document.addEventListener("mousedown", close);
    return () => document.removeEventListener("mousedown", close);
  }, [open]);

  return (
    <div ref={ref} className="relative">
      <button
        onClick={() => setOpen((o) => !o)}
        className="flex cursor-pointer flex-col gap-1 rounded p-1 transition-colors hover:bg-slate-700"
        aria-label="Menu"
      >
        <span className="block h-0.5 w-4 rounded bg-slate-400" />
        <span className="block h-0.5 w-4 rounded bg-slate-400" />
        <span className="block h-0.5 w-4 rounded bg-slate-400" />
      </button>
      {open && (
        <div className="absolute right-0 top-full z-30 mt-4 min-w-36 rounded-xl border border-slate-800 bg-slate-950/70 py-1 shadow-lg">
          {items.map((item) => (
            <button
              key={item.label}
              onClick={() => {
                item.onClick();
                setOpen(false);
              }}
              className={`flex w-full cursor-pointer items-center gap-2 px-3 py-1.5 text-left text-sm transition-colors ${
                item.danger
                  ? "text-red-400 hover:bg-red-900/50"
                  : "text-slate-300 hover:bg-slate-800"
              }`}
            >
              {item.icon && (
                <span className="flex h-4 w-4 items-center justify-center opacity-70">
                  {item.icon}
                </span>
              )}
              {item.label}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
