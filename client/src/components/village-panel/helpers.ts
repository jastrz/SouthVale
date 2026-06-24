export const RESOURCE_ICONS: Record<string, string> = {
  wood: "/icons/resources/wood.png",
  clay: "/icons/resources/clay.png",
  iron: "/icons/resources/iron.png",
  crop: "/icons/resources/crop.png",
};

export function parseTimeSpanMs(ts: string): number {
  const [h, m, s] = ts.split(":").map(Number);
  return ((h || 0) * 3600 + (m || 0) * 60 + (s || 0)) * 1000;
}

export function formatTime(ms: number): string {
  if (ms <= 0) return "Complete!";
  const totalSec = Math.ceil(ms / 1000);
  const h = Math.floor(totalSec / 3600);
  const m = Math.floor((totalSec % 3600) / 60);
  const s = totalSec % 60;
  if (h > 0) return `${h}h ${m.toString().padStart(2, "0")}m`;
  if (m > 0) return `${m}m ${s.toString().padStart(2, "0")}s`;
  return `${s}s`;
}

export function timeRemaining(completesAt: string): number {
  return new Date(completesAt).getTime() - Date.now();
}

