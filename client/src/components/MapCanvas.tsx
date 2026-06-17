import { useEffect, useRef } from "react";
import { Application, Container, Graphics } from "pixi.js";

export function MapCanvas() {
  const divRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!divRef.current) return;

    const app = new Application();
    let mounted = true;

    (async () => {
      await app.init({ resizeTo: divRef.current!, background: "#1a1a2e" });
      if (!mounted) return;

      divRef.current!.appendChild(app.canvas);

      const mapContainer = new Container();
      app.stage.addChild(mapContainer);

      const TILE = 40;
      const COLS = 50;
      const ROWS = 50;

      for (let x = 0; x < COLS; x++) {
        for (let y = 0; y < ROWS; y++) {
          const tile = new Graphics()
            .rect(x * TILE, y * TILE, TILE - 1, TILE - 1)
            .fill(0x1a2a3a);
          mapContainer.addChild(tile);
        }
      }

      mapContainer.x = app.screen.width / 2 - (COLS * TILE) / 2;
      mapContainer.y = app.screen.height / 2 - (ROWS * TILE) / 2;

      let dragging = false;
      let dragStart = { x: 0, y: 0 };
      let containerStart = { x: 0, y: 0 };

      app.canvas.addEventListener("mousedown", (e) => {
        dragging = true;
        dragStart = { x: e.clientX, y: e.clientY };
        containerStart = { x: mapContainer.x, y: mapContainer.y };
      });

      app.canvas.addEventListener("mousemove", (e) => {
        if (!dragging) return;
        mapContainer.x = containerStart.x + (e.clientX - dragStart.x);
        mapContainer.y = containerStart.y + (e.clientY - dragStart.y);
      });

      app.canvas.addEventListener("mouseup", () => (dragging = false));

      app.canvas.addEventListener("wheel", (e) => {
        const factor = e.deltaY > 0 ? 0.9 : 1.1;
        mapContainer.scale.x *= factor;
        mapContainer.scale.y *= factor;
      });
    })();

    return () => {
      mounted = false;
      app.destroy(true);
    };
  }, []);

  return <div ref={divRef} style={{ width: "100vw", height: "100vh" }} />;
}
