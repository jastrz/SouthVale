import { useRef } from "react";
import { useMapRenderer } from "../hooks/useMapRenderer";

export function MapCanvas() {
  const divRef = useRef<HTMLDivElement>(null);
  useMapRenderer(divRef);
  return <div ref={divRef} className="mx-auto h-screen w-[70vw]" />;
}
