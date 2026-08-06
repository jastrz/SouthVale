import { useState } from "react";
import { Outlet } from "@tanstack/react-router";
import { LeaderboardPanel } from "../components/LeaderboardPanel";

export function FrontpageLayout() {
  const [open, setOpen] = useState(false);

  return (
    <div
      className="flex h-screen w-screen flex-col overflow-y-auto bg-slate-950 bg-cover bg-top"
      style={{ backgroundImage: "url(/bg.jpg)", height: "100dvh" }}
    >
      <div className="flex justify-center mt-18">
        <img src="/logo.png" alt="Logo" className="h-32 w-auto"/>
      </div>
      <div className="flex flex-1 flex-col items-center py-16">

        <div className="grid w-full grid-cols-1 gap-8 lg:grid-cols-3">
          <div className="hidden lg:block" />

          <div className="flex justify-center">
            <Outlet />
          </div>

          <div className="flex justify-center">
            <div className="w-full max-w-sm px-4">
              <h2 className="mb-4 text-center text-xl tracking-widest text-white">
                Leaderboard
              </h2>
              <LeaderboardPanel pageSize={10} />
            </div>
          </div>
        </div>
      </div>

      <footer
        className={`flex flex-col items-center gap-1 px-4 py-2 text-center text-xs text-slate-500 bg-slate-950/80 ${open ? "mt-8 w-full" : "self-start rounded-tr"}`}
      >
        <button
          onClick={() => setOpen((v) => !v)}
          className="self-start cursor-pointer text-slate-500 transition-colors hover:text-slate-300"
        >
          {open ? "▾ Hide legal & privacy notice" : "▸ Legal & privacy notice"}
        </button>
        {open && (
          <>
            <p className="mt-3">
              This is a fictional game. Any resemblance to real places, names,
              or events is purely coincidental.
            </p>
            <p>
              <strong className="text-slate-400">Privacy notice</strong>
              <br />
              The service operator (contact below) stores your email address and
              a hashed password solely to create and maintain your game account
              (GDPR Art. 6(1)(b)). No analytics, tracking, or advertising. The
              game features automated AI opponents: your username and village
              names may be processed by an external AI service provider solely
              to generate their gameplay. Data is kept until you delete your
              account. You may access, correct, export, or delete your data at
              any time from the in-game menu or by contacting{" "}
              <a
                href={`mailto:${import.meta.env.VITE_CONTACT_EMAIL ?? "your-email@example.com"}`}
                className="text-slate-400 underline hover:text-slate-300"
              >
                {import.meta.env.VITE_CONTACT_EMAIL ?? "your-email@example.com"}
              </a>
              . You also have the right to lodge a complaint with the President
              of the Personal Data Protection Office (UODO), Poland.
            </p>
            <p>
              <strong className="text-slate-400">User content</strong>
              <br />
              Players are responsible for the content they create (village
              names, usernames, messages). The operator does not pre-moderate
              user content but removes unlawful or offensive content once
              reported. Report such content to{" "}
              <a
                href={`mailto:${import.meta.env.VITE_CONTACT_EMAIL ?? "your-email@example.com"}`}
                className="text-slate-400 underline hover:text-slate-300"
              >
                {import.meta.env.VITE_CONTACT_EMAIL ?? "your-email@example.com"}
              </a>
              .
            </p>
          </>
        )}
      </footer>
    </div>
  );
}
