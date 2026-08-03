export const RESOURCE_ICONS: Record<string, string> = {
  wood: "/icons/resources/wood.png",
  clay: "/icons/resources/clay.png",
  iron: "/icons/resources/iron.png",
  beer: "/icons/resources/beer.png",
};

export const TROOP_ICONS: Record<string, string> = {
  Swordsman: "/icons/troops/swordman.png",
  Archer: "/icons/troops/archer.png",
  Settler: "/icons/troops/hiking.png",
  Dogs: "/icons/troops/dog.png",
  Horsemen: "/icons/troops/horsemen.png",
  LlamaRiders: "/icons/troops/llama_riders.png",
};

export const BUILDING_ICONS: Record<string, string> = {
  WoodCutter: "/icons/buildings/woodcutter.png",
  ClayPit: "/icons/buildings/clay_pit.png",
  IronMine: "/icons/buildings/iron_mine.png",
  Brewery: "/icons/buildings/brewery.png",
  Warehouse: "/icons/buildings/warehouse.png",
  Barracks: "/icons/buildings/barracks.png",
  Stable: "/icons/buildings/stable.png",
  Wall: "/icons/buildings/wall.png",
  Cranny: "/icons/buildings/cranny.png",
  TradePost: "/icons/buildings/trade_post.png",
  TownHall: "/icons/buildings/town_hall.png",
};

export const UI_ICONS: Record<string, string> = {
  map: "/icons/ui/map.png",
  notifications: "/icons/ui/notifications.png",
  leaderboard: "/icons/ui/leaderboard.png",
  info: "/icons/ui/info.png",
  village: "/icons/ui/village.png",
  buildings: "/icons/ui/buildings.png",
  troops: "/icons/ui/troops.png",
  orders: "/icons/ui/orders.png",
  movements: "/icons/ui/movements.png",
  actions: "/icons/ui/actions.png"
};

export function parseTimeSpanMs(ts: string): number {
  const days = ts.includes(".") ? Number(ts.split(".")[0]) : 0;
  const timePart = ts.includes(".") ? ts.split(".")[1] : ts;
  const [h, m, s] = timePart.split(":").map(Number);
  return ((days * 86400 + (h || 0) * 3600 + (m || 0) * 60 + (s || 0)) * 1000);
}

export function formatTime(ms: number): string {
  if (ms <= 0) return "Complete!";
  const totalSec = Math.ceil(ms / 1000);
  const d = Math.floor(totalSec / 86400);
  const h = Math.floor((totalSec % 86400) / 3600);
  const m = Math.floor((totalSec % 3600) / 60);
  const s = totalSec % 60;
  if (d > 0) return `${d}d ${h}h`;
  if (h > 0) return `${h}h ${m.toString().padStart(2, "0")}m`;
  if (m > 0) return `${m}m ${s.toString().padStart(2, "0")}s`;
  return `${s}s`;
}

export function timeRemaining(completesAt: string): number {
  return new Date(completesAt).getTime() - Date.now();
}

const ROLE_CLAIM = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

export function jwtRole(token: string): string | null {
  try {
    const p = JSON.parse(atob(token.split(".")[1]));
    return p[ROLE_CLAIM] ?? p.role ?? null;
  } catch {
    return null;
  }
}

// matches server formula: fields/hour -> seconds
export function travelTime(
  fromX: number,
  fromY: number,
  toX: number,
  toY: number,
  speed: number,
  multiplier: number,
): string {
  const dist = Math.abs(fromX - toX) + Math.abs(fromY - toY);
  const sec = Math.ceil((dist / speed / multiplier) * 3600);
  return formatTime(sec * 1000);
}
