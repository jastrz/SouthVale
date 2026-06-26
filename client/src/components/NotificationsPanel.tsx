import { useReports, useMarkReportsRead } from "../api/hooks/useQueries";

export function NotificationsPanel() {
  const { data } = useReports();
  const markRead = useMarkReportsRead();

  const reports = data?.reports ?? [];

  return (
    <div className="flex w-full max-w-2xl flex-col gap-2 p-4 pt-[15vh] self-start">
      <div className="flex items-center justify-between">
        <h2 className="text-sm font-bold tracking-wide text-slate-300 uppercase">
          Reports
        </h2>
        {data && data.reports.some((r) => !r.isRead) && (
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
          className={`rounded border px-3 py-2 text-xs ${
            r.isRead
              ? "border-slate-800 bg-slate-900/50 text-slate-400"
              : "border-slate-700 bg-slate-800/50 text-slate-200"
          }`}
        >
          <div className="flex items-center justify-between">
            <span className="font-semibold">{r.title}</span>
            <span className="text-[10px] text-slate-500">
              {new Date(r.createdAt).toLocaleString()}
            </span>
          </div>
          <p className="mt-0.5 text-slate-400">{r.body}</p>
        </div>
      ))}
    </div>
  );
}
