import { createContext } from "react";

export interface TravelTimeCtx {
  originX: number;
  originY: number;
  getSpeed: (type: string) => number;
  travelSpeedMultiplier: number;
}

const TravelTimeContext = createContext<TravelTimeCtx | null>(null);

function TravelTimeProvider({
  originX,
  originY,
  troopSpeeds,
  travelSpeedMultiplier = 50,
  children,
}: {
  originX: number;
  originY: number;
  troopSpeeds?: Record<string, number>;
  travelSpeedMultiplier?: number;
  children: React.ReactNode;
}) {
  const value: TravelTimeCtx = {
    originX,
    originY,
    getSpeed: (type) => troopSpeeds?.[type] ?? 5,
    travelSpeedMultiplier,
  };
  return (
    <TravelTimeContext.Provider value={value}>
      {children}
    </TravelTimeContext.Provider>
  );
}

export { TravelTimeContext, TravelTimeProvider };
