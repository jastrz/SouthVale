import { Link, useNavigate } from "@tanstack/react-router";
import { useAuthStore } from "../store/authStore";

export function LoginBar() {
  const token = useAuthStore((s) => s.token);
  const username = useAuthStore((s) => s.username);
  const email = useAuthStore((s) => s.email);
  const clearAuth = useAuthStore((s) => s.clearAuth);
  const navigate = useNavigate();

  const handleLogout = () => {
    clearAuth();
    navigate({ to: "/login" });
  };

  return (
    <div
      className="fixed top-5 right-5 z-50 flex items-center gap-3 rounded-lg border border-[#2a2a3e] bg-[rgba(15,15,30,0.85)] px-3.5 py-2 font-sans text-base text-white"
    >
      {token ? (
        <>
          <span className="text-[#a0a0b0]">{username ?? email ?? "Logged in"}</span>
          <button
            onClick={handleLogout}
            className="cursor-pointer rounded-md border-none bg-[#d32f2f] px-3 py-1.5 text-sm font-bold text-white"
          >
            Logout
          </button>
        </>
      ) : (
        <>
          <Link
            to="/login"
            className="rounded-md bg-[#2196F3] px-3 py-1.5 text-sm font-bold text-white no-underline"
          >
            Login
          </Link>
          <Link
            to="/register"
            className="rounded-md bg-[#4CAF50] px-3 py-1.5 text-sm font-bold text-white no-underline"
          >
            Register
          </Link>
        </>
      )}
    </div>
  );
}
