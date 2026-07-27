import { useState } from "react";
import { useLeaderboard } from "../api/hooks/useQueries";
import { Pagination } from "./Pagination";
import { useAuthStore } from "../store/authStore";

export function LeaderboardPanel() {
  const [page, setPage] = useState(1);
  const { data } = useLeaderboard(page);
  const username = useAuthStore((s) => s.username);

  const totalPages = Math.ceil((data?.totalCount ?? 0) / 10);

  return (
    <div className="flex flex-col gap-3">
      {/*<h2 className="text-xs font-bold tracking-widest text-slate-400 uppercase text-center">
        Leaderboard
      </h2>*/}
      <div className="rounded-xl overflow-hidden">
        <table className="w-full text-left text-xs">
          <thead>
            <tr className="text-slate-400 uppercase tracking-wider bg-slate-800/80">
              <th className="pl-2 pr-4 py-2">#</th>
              <th>Player</th>
              <th className="pr-2 text-right">Score</th>
            </tr>
          </thead>
          <tbody>
            {data?.items.map((entry) => (
              <tr
                key={entry.playerId}
                className="border-t border-slate-700 bg-slate-800/80 text-white"
              >
                <td className="py-1.5 px-2">{entry.rank}</td>
                <td
                  className={`py-1.5 ${entry.username === username ? "text-amber-600" : ""}`}
                >
                  {entry.username}
                </td>
                <td className="py-1.5 px-2 text-right">
                  {entry.score.toLocaleString()}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

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
