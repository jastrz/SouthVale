import { Outlet } from "@tanstack/react-router";
import { LeaderboardPanel } from "../components/LeaderboardPanel";

export function FrontpageLayout() {
  return (
    <div
      className="flex h-screen w-screen flex-col overflow-y-auto bg-slate-950 bg-cover bg-top"
      style={{ backgroundImage: "url(/bg.png)" }}
    >
      <div className="flex flex-col items-center gap-2 px-4 py-2">
        <img
          src="/logo.png"
          alt="Logo"
          className="h-80 w-auto drop-shadow-[0_0_32px_rgba(0,0,0,1)]"
        />

        <div className="grid w-full grid-cols-1 gap-8 lg:grid-cols-3">
          <div className="hidden lg:block" />

          <div className="flex justify-center">
            <Outlet />
          </div>

          <div className="flex justify-center">
            <div className="w-full max-w-sm">
              <h2 className="mb-4 text-center text-xl font-bold tracking-widest text-orange-700 uppercase">
                Leaderboard
              </h2>
              <div className="rounded-xl bg-slate-800/90 p-4 shadow-2xl">
                <LeaderboardPanel />
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
