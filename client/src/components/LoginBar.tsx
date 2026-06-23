import { useAuthStore } from "../store/authStore";
import { useGameStateStore } from "../store/gameStateStore";
import { useNavigate, Link } from "@tanstack/react-router";
import { api } from "../lib/axios";

export function LoginBar() {
  const token = useAuthStore((s) => s.token);
  const username = useAuthStore((s) => s.username);
  const email = useAuthStore((s) => s.email);
  const clearAuth = useAuthStore((s) => s.clearAuth);
  const clearGameState = useGameStateStore((s) => s.clear);
  const navigate = useNavigate();

  const handleLogout = async () => {
    await api.post("/auth/logout").catch(() => {});
    clearAuth();
    clearGameState();
    navigate({ to: "/login" });
  };

  return (
    <div className="fixed bottom-5 left-5 z-50 flex items-center gap-3 rounded-lg border border-slate-800 bg-slate-950/85 px-3.5 py-2 font-sans text-base text-white">
      {token ? (
        <>
          <span className="text-slate-400">
            {username ?? email ?? "Logged in"}
          </span>
          <button
            onClick={handleLogout}
            className="cursor-pointer rounded-md border-none bg-red-700 px-3 py-1.5 text-sm font-bold text-white hover:bg-red-600 transition-colors"
          >
            Logout
          </button>
        </>
      ) : (
        <>
          <Link
            to="/login"
            className="rounded-md bg-blue-600 px-3 py-1.5 text-sm font-bold text-white no-underline hover:bg-blue-500 transition-colors"
          >
            Login
          </Link>
          <Link
            to="/register"
            className="rounded-md bg-green-600 px-3 py-1.5 text-sm font-bold text-white no-underline hover:bg-green-500 transition-colors"
          >
            Register
          </Link>
        </>
      )}
    </div>
  );
}
