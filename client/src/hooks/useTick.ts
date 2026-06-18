import { useState, useEffect } from "react";

/**
 * Forces its caller to re-render every `intervalMs` so countdown displays
 * tick down in real time without server push.
 */
export function useTick(intervalMs = 1000) {
  const [tick, setTick] = useState(0);
  useEffect(() => {
    const id = setInterval(() => setTick((t) => t + 1), intervalMs);
    return () => clearInterval(id);
  }, [intervalMs]);
  return tick;
}
