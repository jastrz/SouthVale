import { Container, Graphics, Sprite } from "pixi.js";
import type { Application } from "pixi.js";
import type { MovementDto, Coordinates } from "../../../api/types";
import { COLORS, MOVEMENT, TILE_SIZE } from "../../config";
import { movementIcon } from "../../atlas";

function drawDashedLine(g: Graphics, x1: number, y1: number, x2: number, y2: number): void {
  const dx = x2 - x1;
  const dy = y2 - y1;
  const dist = Math.hypot(dx, dy);
  if (dist < 1) return;
  const nx = dx / dist;
  const ny = dy / dist;
  let drawn = 0;
  while (drawn < dist) {
    const end = Math.min(drawn + MOVEMENT.dash, dist);
    g.moveTo(x1 + nx * drawn, y1 + ny * drawn);
    g.lineTo(x1 + nx * end, y1 + ny * end);
    drawn = end + MOVEMENT.gap;
  }
}

function drawArrow(g: Graphics, x1: number, y1: number, x2: number, y2: number, color: number): void {
  const angle = Math.atan2(y2 - y1, x2 - x1);
  const barb = Math.PI / 6;
  const s = MOVEMENT.arrowSize;
  g.poly([
    x2, y2,
    x2 - s * Math.cos(angle - barb), y2 - s * Math.sin(angle - barb),
    x2 - s * Math.cos(angle + barb), y2 - s * Math.sin(angle + barb),
  ]);
  g.fill({ color, alpha: MOVEMENT.arrowAlpha });
}

function lineColor(type: string, isOutgoing: boolean): number {
  if (type === "Attack") return isOutgoing ? COLORS.movement.attackOutgoing : COLORS.movement.attackIncoming;
  if (type === "Settle") return COLORS.movement.settle;
  return isOutgoing ? COLORS.movement.transportOutgoing : COLORS.movement.transportIncoming;
}

interface MovementVisual {
  from: Coordinates;
  to: Coordinates;
  departureMs: number;
  arrivesMs: number;
  icon: Sprite;
  container: Container;
}

export class MovementLayer extends Container {
  private visuals = new Map<string, MovementVisual>();
  private tickerRegistered = false;
  private app: Application;

  constructor(app: Application) {
    super();
    this.label = "MovementLayer";
    this.app = app;
  }

  setMovements(
    movements: readonly MovementDto[],
    villageCoords: Record<string, Coordinates>,
    ownIds: Set<string>,
  ): void {
    this.removeChildren().forEach((c) => c.destroy());
    this.visuals.clear();
    this.unregisterTicker();

    for (const m of movements) {
      if (m.status !== "InFlight") continue;

      const isReturn = m.type === "Return";
      const isSettle = m.type === "Settle";
      const home = villageCoords[m.originVillageId];
      const target = m.targetVillageId
        ? villageCoords[m.targetVillageId]
        : m.targetCoordinates;
      if (!home || !target) continue;

      const fromCoord = isReturn ? target : home;
      const toCoord = isReturn ? home : target;
      const isOutgoing = !isReturn && ownIds.has(m.originVillageId);
      const iconType: "Attack" | "Transport" | "Settle" = isSettle ? "Settle" : m.type === "Attack" ? "Attack" : "Transport";

      const x1 = fromCoord.x * TILE_SIZE + TILE_SIZE / 2;
      const y1 = fromCoord.y * TILE_SIZE + TILE_SIZE / 2;
      const x2 = toCoord.x * TILE_SIZE + TILE_SIZE / 2;
      const y2 = toCoord.y * TILE_SIZE + TILE_SIZE / 2;

      const clr = lineColor(m.type, isOutgoing);
      const line = new Graphics();
      drawDashedLine(line, x1, y1, x2, y2);
      line.stroke({ width: MOVEMENT.lineWidth, color: clr, alpha: MOVEMENT.lineAlpha });
      drawArrow(line, x1, y1, x2, y2, clr);

      const icon = new Sprite(movementIcon(iconType));
      icon.anchor.set(0.5);
      icon.scale.set(MOVEMENT.iconScale);

      const c = new Container();
      c.addChild(line);
      c.addChild(icon);
      this.addChild(c);

      this.visuals.set(m.id, {
        from: fromCoord,
        to: toCoord,
        departureMs: new Date(m.departureAt).getTime(),
        arrivesMs: new Date(m.arrivesAt).getTime(),
        icon,
        container: c,
      });
    }

    if (this.visuals.size > 0) {
      this.registerTicker();
    }
  }

  private update = (): void => {
    const now = Date.now();
    for (const v of this.visuals.values()) {
      const elapsed = now - v.departureMs;
      const total = v.arrivesMs - v.departureMs;
      const t = total > 0 ? Math.min(elapsed / total, 1) : 1;

      const x1 = v.from.x * TILE_SIZE + TILE_SIZE / 2;
      const y1 = v.from.y * TILE_SIZE + TILE_SIZE / 2;
      const x2 = v.to.x * TILE_SIZE + TILE_SIZE / 2;
      const y2 = v.to.y * TILE_SIZE + TILE_SIZE / 2;

      v.icon.x = x1 + (x2 - x1) * t;
      v.icon.y = y1 + (y2 - y1) * t;

      const pulse = 1 + Math.sin(elapsed * 0.005) * 0.05;
      v.icon.scale.set(MOVEMENT.iconScale * pulse);
    }
  };

  private registerTicker(): void {
    if (!this.tickerRegistered) {
      this.app.ticker.add(this.update);
      this.tickerRegistered = true;
    }
  }

  private unregisterTicker(): void {
    if (this.tickerRegistered) {
      this.app.ticker.remove(this.update);
      this.tickerRegistered = false;
    }
  }

  destroy(): void {
    this.unregisterTicker();
    super.destroy({ children: true });
  }
}
