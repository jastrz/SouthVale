import { useRef } from "react";
import { useMapRenderer } from "../hooks/useMapRenderer";

export function MapCanvas() {
  const divRef = useRef<HTMLDivElement>(null);
  useMapRenderer(divRef);
  return <div ref={divRef} className="h-screen w-full" />;
}
