import { useState } from "react";
import { Link, useNavigate } from "@tanstack/react-router";
import { useAuthStore } from "../store/authStore";
import { useGameStateStore } from "../store/gameStateStore";
import { api } from "../lib/axios";
import { jwtRole } from "../lib/helpers";
import { useDelete } from "../api/hooks/useAuth";
import { BurgerMenu } from "./BurgerMenu";
import { ClaimAccountModal } from "./ClaimAccountModal";

export function LoginBar() {
  const navigate = useNavigate();
  const token = useAuthStore((s) => s.token);
  const username = useAuthStore((s) => s.username);
  const email = useAuthStore((s) => s.email);
  const clearAuth = useAuthStore((s) => s.clearAuth);
  const clearGameState = useGameStateStore((s) => s.clear);
  const deleteMutation = useDelete();
  const [showClaim, setShowClaim] = useState(false);

  const isAdmin = token ? jwtRole(token) === "Admin" : false;
  const isGuest = token && !email;

  const handleLogout = async () => {
    await api.post("/auth/logout").catch(() => {});
    clearAuth();
    clearGameState();
    navigate({ to: "/login" });
  };

  const handleDelete = () => {
    const password = window.prompt("Enter your password to confirm deletion:");
    if (!password) return;
    deleteMutation.mutate(
      { password },
      {
        onSuccess: () => {
          clearAuth();
          clearGameState();
          navigate({ to: "/login" });
        },
        onError: () => {
          alert("Deletion failed. Check your password.");
        },
      },
    );
  };

  const menuItems = isAdmin
    ? [{ label: "Logout", onClick: handleLogout }]
    : [
        ...(isGuest ? [{ label: "Claim Account", onClick: () => setShowClaim(true) }] : []),
        { label: "Logout", onClick: handleLogout },
        ...(isGuest ? [] : [{ label: "Delete account", onClick: handleDelete, danger: true }]),
      ];

  return (
    <div className="flex items-center gap-2">
      {token ? (
        <>
          <img
            src="/characters/character_040.png"
            alt=""
            className="h-8 w-auto"
          />
          <span className="max-w-30 truncate text-slate-200">
            {username ?? email}
          </span>
          <BurgerMenu items={menuItems} />
          {showClaim && <ClaimAccountModal onClose={() => setShowClaim(false)} />}
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
