import { PANEL_CLAMP } from "../../pixi/config";
import type { ReactNode } from "react";
import { ScrollArea } from "./ScrollArea";

export function PanelContainer({ side, children }: { side?: "left" | "right"; children: ReactNode }) {
  return (
    <div
      className={`flex flex-none h-screen flex-col overflow-hidden font-sans text-white backdrop-blur-xs bg-slate-800/30 pointer-events-auto ${side === "right" ? "rounded-l-xl" : "rounded-r-xl"}`}
      style={{ width: PANEL_CLAMP, height: "100dvh" }}
    >
      <ScrollArea className="flex-1 min-h-0">{children}</ScrollArea>
    </div>
  );
}
