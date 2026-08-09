import { useSyncExternalStore } from "react";
import { requestBusy } from "../lib/requestBusy";
import { Spinner } from "./Spinner";

export function GlobalSpinner() {
  const busy = useSyncExternalStore(requestBusy.subscribe, requestBusy.get);
  return (
    <div className={`pointer-events-none fixed right-3 top-3 z-50 transition-opacity duration-200 ${busy ? "opacity-100" : "opacity-0"}`}>
      <Spinner size={14} className="border-slate-500 border-t-slate-200" />
    </div>
  );
}
