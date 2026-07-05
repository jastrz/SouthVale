import { useState } from "react";
import { useLeaderboard } from "../api/hooks/useQueries";
import { Pagination } from "./Pagination";
import { useAuthStore } from "../store/authStore";

export function LeaderboardPanel() {
  const [page, setPage] = useState(1);
  const { data } = useLeaderboard(page);
  const username = useAuthStore((s) => s.username);

  const totalPages = Math.ceil((data?.totalCount ?? 0) / 20);

  return (
    <div className="flex flex-col gap-3">
      <table className="w-full text-left text-xs">
        <thead>
          <tr className="text-slate-500 uppercase tracking-wider">
            <th className="pb-2 pr-4">#</th>
            <th className="pb-2">Player</th>
            <th className="pb-2 text-right">Score</th>
          </tr>
        </thead>
        <tbody>
          {data?.items.map((entry) => (
            <tr
              key={entry.playerId}
              className="border-t border-slate-700 text-slate-200"
            >
              <td className="py-1.5 pr-4 text-slate-500">{entry.rank}</td>
              <td
                className={`py-1.5 ${entry.username === username ? "text-amber-300" : ""}`}
              >
                {entry.username}
              </td>
              <td className="py-1.5 text-right">
                {entry.score.toLocaleString()}
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      {!data && (
        <p className="text-center text-xs text-slate-500">Loading...</p>
      )}

      {data?.items.length === 0 && (
        <p className="text-center text-xs text-slate-500">No players yet.</p>
      )}

      {totalPages > 1 && (
        <Pagination page={page} totalPages={totalPages} onChange={setPage} />
      )}
    </div>
  );
}
