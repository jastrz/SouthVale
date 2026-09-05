import { PANEL_CLAMP } from "../pixi/config";
import type { ReactNode } from "react";

export function PanelContainer({ side, children }: { side?: "left" | "right"; children: ReactNode }) {
  return (
    <div
      className={`scrollbar-hide flex-none h-screen flex-col overflow-y-auto font-sans text-white backdrop-blur-xs bg-slate-800/30 pointer-events-auto ${side === "right" ? "rounded-l-xl" : "rounded-r-xl"}`}
      style={{ width: PANEL_CLAMP, height: "100dvh" }}
    >
      {children}
    </div>
  );
}
