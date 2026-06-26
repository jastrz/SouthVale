import { useState } from "react";
import {
  useReports,
  useMarkReportsRead,
  useMarkReportRead,
} from "../api/hooks/useQueries";
import type { ReportDto } from "../api/types";

// Temporarily this ugly solution...
function titleColor(r: ReportDto): string {
  if (r.type === "Attack" || r.type === "Defense") {
    const lost = r.body.includes("None of") || r.body.includes("No defenders");
    return lost ? "text-rose-400" : "text-emerald-400";
  }
  return "text-amber-400";
}

export function NotificationsPanel() {
  const [page, setPage] = useState(1);
  const { data } = useReports(page);
  const markRead = useMarkReportsRead();
  const markOne = useMarkReportRead();

  const reports = data?.reports ?? [];
  const unreadCount = data?.unreadCount ?? 0;
  const totalPages = Math.ceil((data?.totalCount ?? 0) / 10);

  return (
    <div className="flex w-full max-w-2xl flex-col gap-2 p-4 pt-[15vh] self-start">
      <div className="flex items-center justify-between">
        <h2 className="text-sm font-bold tracking-wide text-slate-300 uppercase">
          Reports
        </h2>
        {unreadCount > 0 && (
          <button
            className="rounded bg-slate-700 px-2 py-0.5 text-xs text-slate-300 hover:bg-slate-600"
            onClick={() => markRead.mutate()}
          >
            Mark all as read
          </button>
        )}
      </div>
      {reports.length === 0 && (
        <p className="mt-8 text-center text-xs text-slate-500">
          No reports yet.
        </p>
      )}
      {reports.map((r) => (
        <div
          key={r.id}
          onClick={() => {
            if (!r.isRead) markOne.mutate(r.id);
          }}
          className={`cursor-pointer rounded border px-3 py-2 text-xs ${
            r.isRead
              ? "border-slate-800 bg-slate-900/50 text-slate-400"
              : "border-slate-700 bg-slate-800/50 text-slate-200"
          }`}
        >
          <div className="flex items-center justify-between">
            <span className={`font-semibold ${titleColor(r)}`}>{r.title}</span>
            <span className="text-[10px] text-slate-500">
              {new Date(r.createdAt).toLocaleString()}
            </span>
          </div>
          <p className="mt-0.5 whitespace-pre-line text-slate-400">{r.body}</p>
        </div>
      ))}
      {totalPages > 1 && (
        <div className="mt-2 flex items-center justify-center gap-2">
          <button
            className="rounded bg-slate-700 px-2 py-0.5 text-xs text-slate-300 hover:bg-slate-600 disabled:opacity-40"
            onClick={() => setPage((p) => p - 1)}
            disabled={page <= 1}
          >
            Prev
          </button>
          <span className="text-xs text-slate-400">
            {page} / {totalPages}
          </span>
          <button
            className="rounded bg-slate-700 px-2 py-0.5 text-xs text-slate-300 hover:bg-slate-600 disabled:opacity-40"
            onClick={() => setPage((p) => p + 1)}
            disabled={page >= totalPages}
          >
            Next
          </button>
        </div>
      )}
    </div>
  );
}
