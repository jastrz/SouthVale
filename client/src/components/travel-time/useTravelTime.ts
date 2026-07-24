import { useContext } from "react";
import { travelTime } from "../../lib/helpers";
import { TravelTimeContext } from "./TravelTime";

export function useTravelTime() {
  const ctx = useContext(TravelTimeContext);
  if (!ctx) throw new Error("useTravelTime needs TravelTimeProvider");
  return {
    getEta: (toX: number, toY: number, speed: number) =>
      travelTime(ctx.originX, ctx.originY, toX, toY, speed, ctx.travelSpeedMultiplier),
    getSpeed: ctx.getSpeed,
  };
}
