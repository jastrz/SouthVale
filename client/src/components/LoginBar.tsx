import { Link, useNavigate } from "@tanstack/react-router";
import { useAuthStore } from "../store/authStore";
import { useGameStateStore } from "../store/gameStateStore";
import { api } from "../lib/axios";

export function LoginBar() {
  const navigate = useNavigate();
  const token = useAuthStore((s) => s.token);
  const username = useAuthStore((s) => s.username);
  const email = useAuthStore((s) => s.email);
  const clearAuth = useAuthStore((s) => s.clearAuth);
  const clearGameState = useGameStateStore((s) => s.clear);

  const handleLogout = async () => {
    await api.post("/auth/logout").catch(() => {});
    clearAuth();
    clearGameState();
    navigate({ to: "/login" });
  };

  return (
    <div className="flex items-center gap-2">
      {token ? (
        <>
          <span className="max-w-[120px] truncate text-slate-200">{username ?? email}</span>
          <button
            onClick={handleLogout}
            className="cursor-pointer rounded bg-red-700 px-2 py-1 font-bold text-white transition-colors hover:bg-red-600"
          >
            Logout
          </button>
        </>
      ) : (
        <>
          <Link
            to="/login"
            className="rounded bg-blue-600 px-2 py-1 font-bold text-white no-underline transition-colors hover:bg-blue-500"
          >
            Login
          </Link>
          <Link
            to="/register"
            className="rounded bg-green-600 px-2 py-1 font-bold text-white no-underline transition-colors hover:bg-green-500"
          >
            Register
          </Link>
        </>
      )}
    </div>
  );
}
