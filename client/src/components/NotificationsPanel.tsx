import { useState } from "react";
import { useReports, useMarkReportsRead } from "../api/hooks/useQueries";
import { Pagination } from "./Pagination";
import { Report } from "./Report";

export function NotificationsPanel() {
  const [page, setPage] = useState(1);
  const { data } = useReports(page);
  const markRead = useMarkReportsRead();

  const reports = data?.reports ?? [];
  const unreadCount = data?.unreadCount ?? 0;
  const totalPages = Math.ceil((data?.totalCount ?? 0) / 10);

  return (
    <div className="flex w-full max-w-2xl h-full flex-col gap-2 p-4 pt-24">
      <div className="flex items-center">
        <div className="flex-1" />
        <h2 className="text-sm font-bold tracking-wide text-slate-300 uppercase">
          Reports
        </h2>
        <div className="flex-1 flex justify-end">
          {unreadCount > 0 && (
            <button
              className="rounded bg-slate-700 px-2 py-0.5 text-xs text-slate-300 hover:bg-slate-600"
              onClick={() => markRead.mutate()}
            >
              Mark all as read
            </button>
          )}
        </div>
      </div>

      <div className="flex-1 overflow-y-auto space-y-2 min-h-0">
        {reports.length === 0 && (
          <p className="mt-8 text-center text-xs text-slate-500">
            No reports yet.
          </p>
        )}
        {reports.map((r) => (
          <Report key={r.id} report={r} />
        ))}
      </div>
      {totalPages > 1 && (
        <Pagination page={page} totalPages={totalPages} onChange={setPage} />
      )}
    </div>
  );
}
