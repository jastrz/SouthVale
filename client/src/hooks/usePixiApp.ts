import { Application } from "pixi.js";
import { useEffect, useRef } from "react";

export function usePixiApp() {
  const appRef = useRef<Application | null>(null);

  useEffect(() => {
    return () => {
      if (appRef.current) {
        appRef.current.destroy(true);
        appRef.current = null;
      }
    };
  }, []);

  return appRef;
}
