import { createContext } from "react";

export interface TravelTimeCtx {
  originX: number;
  originY: number;
  getSpeed: (type: string) => number;
}

const TravelTimeContext = createContext<TravelTimeCtx | null>(null);

function TravelTimeProvider({
  originX,
  originY,
  troopSpeeds,
  children,
}: {
  originX: number;
  originY: number;
  troopSpeeds?: Record<string, number>;
  children: React.ReactNode;
}) {
  const value: TravelTimeCtx = {
    originX,
    originY,
    getSpeed: (type) => troopSpeeds?.[type] ?? 5,
  };
  return (
    <TravelTimeContext.Provider value={value}>
      {children}
    </TravelTimeContext.Provider>
  );
}

export { TravelTimeContext, TravelTimeProvider };
