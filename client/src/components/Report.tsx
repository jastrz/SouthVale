import { useMarkReportRead } from "../api/hooks/useQueries";
import type { ReportDto } from "../api/types";

// Temporarily this ugly solution...
function titleColor(r: ReportDto): string {
  if (r.type === "Attack" || r.type === "Defense") {
    const lost = r.body.includes("None of") || r.body.includes("No defenders");
    return lost ? "text-rose-400" : "text-emerald-400";
  }
  return "text-amber-400";
}

type ReportProps = { report: ReportDto };

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
      <p className="mt-0.5 whitespace-pre-line text-slate-400">{report.body}</p>
    </div>
  );
}
