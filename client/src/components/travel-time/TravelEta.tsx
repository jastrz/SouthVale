import { useContext } from "react";
import { travelTime } from "../../lib/helpers";
import { TravelTimeContext } from "./TravelTime";

export function TravelEta({
  toX,
  toY,
  speed,
}: {
  toX: number;
  toY: number;
  speed: number;
}) {
  const ctx = useContext(TravelTimeContext);
  if (!ctx) return null;
  return (
    <div className="text-yellow-400">
      time: {travelTime(ctx.originX, ctx.originY, toX, toY, speed)}
    </div>
  );
}
