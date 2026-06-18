import type { ReactNode } from "react";

export function PanelContainer({ children }: { children: ReactNode }) {
  return (
    <div className="flex h-screen w-80 flex-col overflow-y-auto border-l border-slate-800 bg-slate-950/90 font-sans text-white backdrop-blur-sm">
      {children}
    </div>
  );
}
