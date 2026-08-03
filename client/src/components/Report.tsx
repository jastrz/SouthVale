import { useMarkReportRead } from "../api/hooks/useQueries";
import type { ReportDto } from "../api/types";
import { TROOP_ICONS, RESOURCE_ICONS } from "../lib/helpers";
import { Icon } from "./Icon";

// Temporarily this ugly solution...
function titleColor(r: ReportDto): string {
  if (r.type === "Attack" || r.type === "Defense") {
    const block =
      r.type === "Attack"
        ? r.body.split("Defenders:")[0] ?? ""
        : r.body.split("Defenders:")[1] ?? r.body;
    const newFmt = block.match(/Survived: (.*)/);
    if (newFmt) return newFmt[1].trim() === "none" ? "text-rose-400" : "text-emerald-400";
    // old reports: "Attackers: X of Y survived."
    const att = r.body.match(/Attackers: (.*?) of (.*?) survived/);
    const def = r.body.match(/Defenders: (.*?) of (.*?) survived/);
    const mySurvivors = (r.type === "Attack" ? att?.[1] : def?.[1]) ?? "None";
    return mySurvivors.trim() === "None" ? "text-rose-400" : "text-emerald-400";
  }
  return "text-amber-400";
}

type ReportProps = { report: ReportDto };

type CombatRow = { type: string; sent: number; lost: number; survived: number };

function parseEntryList(line: string | undefined): { name: string; count: number }[] {
  if (!line) return [];
  const out: { name: string; count: number }[] = [];
  const re = /(\d+)\s+([A-Za-z]+)/g;
  let m: RegExpExecArray | null;
  while ((m = re.exec(line))) out.push({ name: m[2], count: Number(m[1]) });
  return out;
}

function parseSide(block: string): CombatRow[] {
  const lines = block.split("\n");
  const get = (label: string) =>
    lines.find((l) => l.startsWith(label + ":"))?.slice(label.length + 1);
  const lostMap = new Map(parseEntryList(get("Lost")).map((e) => [e.name, e.count]));
  const survMap = new Map(parseEntryList(get("Survived")).map((e) => [e.name, e.count]));
  return parseEntryList(get("Sent")).map((e) => ({
    type: e.name,
    sent: e.count,
    lost: lostMap.get(e.name) ?? 0,
    survived: survMap.get(e.name) ?? 0,
  }));
}

function combatTables(body: string) {
  const [att, def] = body.split("Defenders:");
  if (!att?.includes("Sent:")) return null;
  const loot = body.split("\n").find((l) => l.startsWith("Loot:"));
  const header = body
    .split("\n")
    .filter((l) =>
      l.startsWith("Source:") || l.startsWith("Target:") ||
      l.startsWith("Attacker:") || l.startsWith("Defender:"),
    );
  return { attackerRows: parseSide(att), defenderRows: parseSide(def ?? ""), loot, header };
}

function parseHeader(line: string): { village: string; player: string } {
  const v = line.replace(/^(Source|Target|Attacker|Defender): /, "");
  const m = v.match(/^(.*) \((.*)\)$/);
  return m ? { village: m[1], player: m[2] } : { village: v, player: "" };
}

function SideTable({
  title,
  village,
  player,
  rows,
}: {
  title: string;
  village: string;
  player: string;
  rows: CombatRow[];
}) {
  if (rows.length === 0) return null;
  return (
    <div className="mt-4">
      <div className="text-slate-300">
        {title} - <span className="text-white">{player}</span>
        <span className="text-slate-400"> ({village})</span>
      </div>
      <table className="w-full table-fixed mt-1">
        <thead>
          <tr className="text-[10px] text-slate-500">
            <th className="text-left">Troop</th>
            <th className="w-14 text-right">Sent</th>
            <th className="w-14 text-right">Lost</th>
            <th className="w-16 text-right">Survived</th>
          </tr>
        </thead>
        <tbody>
          {rows.map((r) => (
            <tr key={r.type} className="text-slate-300">
              <td className="truncate">
                <span className="flex items-center gap-1">
                  {TROOP_ICONS[r.type] && (
                    <Icon src={TROOP_ICONS[r.type]} size={14} />
                  )}
                  {r.type}
                </span>
              </td>
              <td className="w-14 text-right">{r.sent}</td>
              <td className={`w-14 text-right ${r.lost > 0 ? "text-rose-400" : ""}`}>{r.lost}</td>
              <td className="w-16 text-right text-emerald-400">{r.survived}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

function InlineIcons({ text }: { text: string }) {
  const parts = text.split(/(\d+ [A-Za-z]+)/g);
  return (
    <span className="inline-flex flex-wrap items-center whitespace-pre">
      {parts.map((p, i) => {
        const m = p.match(/^(\d+) ([A-Za-z]+)$/);
        const icon = m
          ? TROOP_ICONS[m[2]] ?? RESOURCE_ICONS[m[2].toLowerCase()]
          : undefined;
        return m && icon ? (
          <span key={i} className="inline-flex items-center whitespace-nowrap">
            <Icon src={icon} size={12} />
            <span className="pl-0.5">{p}</span>
          </span>
        ) : (
          <span key={i}>{p}</span>
        );
      })}
    </span>
  );
}

function RichBody({ body }: { body: string }) {
  return (
    <div>
      {body.split("\n").map((line, i) => {
        if (!line) return <div key={i} className="h-1" />;
        const [label, ...rest] = line.split(": ");
        return (
          <div key={i} className="text-slate-400">
            {rest.length > 0 && label.length < 30 && (
              <span className="text-slate-500">{label}: </span>
            )}
            <InlineIcons text={rest.length > 0 ? rest.join(": ") : line} />
          </div>
        );
      })}
    </div>
  );
}

export function Report({ report }: ReportProps) {
  const markOne = useMarkReportRead();

  return (
    <div
      onClick={() => {
        if (!report.isRead) markOne.mutate(report.id);
      }}
      className={`cursor-pointer rounded-xl border px-3 py-2 text-xs ${
        report.isRead
          ? "border-slate-800 bg-slate-800/80 text-slate-400"
          : "border-slate-700 bg-slate-800/95 text-slate-200"
      }`}
    >
      <div className="flex items-center justify-between">
        <span className={`font-semibold ${titleColor(report)}`}>
          {report.title}
        </span>
        <span className="text-[10px] text-slate-500">
          {new Date(report.createdAt).toLocaleString()}
        </span>
      </div>
      {(() => {
        const tables = combatTables(report.body);
        if (!tables) return <RichBody body={report.body} />;
        return (
          <div>
            <SideTable
              title="Attackers"
              {...parseHeader(tables.header.find((l) => l.startsWith("Source:")) ?? tables.header.find((l) => l.startsWith("Attacker:")) ?? "")}
              rows={tables.attackerRows}
            />
            <SideTable
              title="Defenders"
              {...parseHeader(tables.header.find((l) => l.startsWith("Target:")) ?? tables.header.find((l) => l.startsWith("Defender:")) ?? "")}
              rows={tables.defenderRows}
            />
            {tables.loot && (
              <div className="mt-4 text-slate-400">
                <InlineIcons text={tables.loot} />
              </div>
            )}
          </div>
        );
      })()}
    </div>
  );
}
