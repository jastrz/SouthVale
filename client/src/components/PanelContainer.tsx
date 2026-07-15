import { PANEL_CLAMP } from "../pixi/config";
import type { ReactNode } from "react";

export function PanelContainer({ children }: { children: ReactNode }) {
  return (
    <div
      className="flex-none h-screen flex-col overflow-y-scroll font-sans text-white backdrop-blur-sm bg-slate-800/70 pointer-events-auto"
      style={{ width: PANEL_CLAMP, height: "100dvh" }}
    >
      {children}
    </div>
  );
}
