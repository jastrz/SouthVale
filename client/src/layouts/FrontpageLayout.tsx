import { Outlet } from "@tanstack/react-router";

export function FrontpageLayout() {
  return (
    <div
      className="flex h-screen w-screen flex-col overflow-y-auto bg-slate-950 bg-cover bg-center"
      style={{ backgroundImage: "url(/bg.png)" }}
    >
      <div className="flex flex-1 flex-col items-center justify-top gap-2 px-4 py-2">
        <img
          src="/logo.png"
          alt="Logo"
          className="h-80 w-auto drop-shadow-[0_0_32px_rgba(0,0,0,1)]"
        />
        <Outlet />
      </div>
    </div>
  );
}
