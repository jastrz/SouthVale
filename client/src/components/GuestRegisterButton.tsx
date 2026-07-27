import { useNavigate } from "@tanstack/react-router";
import type { AxiosError } from "axios";
import { useRegisterGuest } from "../api/hooks/useAuth";
import { useAuthStore } from "../store/authStore";

export function GuestRegisterButton() {
  const navigate = useNavigate();
  const setAuth = useAuthStore((s) => s.setAuth);
  const registerGuest = useRegisterGuest();

  const guestError =
    (registerGuest.error as AxiosError<{ detail?: string }> | undefined)?.response?.data?.detail ??
    registerGuest.error?.message ??
    null;

  return (
    <>
      <div className="relative my-1 flex items-center gap-2">
        <div className="flex-1 border-t border-slate-600" />
        <span className="text-xs text-slate-500">or</span>
        <div className="flex-1 border-t border-slate-600" />
      </div>

      {registerGuest.isError && guestError && (
        <div className="rounded-md bg-red-950 px-3 py-2 text-sm text-red-400">{guestError}</div>
      )}

      <button
        type="button"
        onClick={() => {
          registerGuest.mutate(undefined, {
            onSuccess: (response) => {
              setAuth(response.data.accessToken, undefined, response.data.username);
              navigate({ to: "/" });
            },
          });
        }}
        disabled={registerGuest.isPending}
        className="cursor-pointer rounded-md bg-slate-700 py-2 text-sm font-semibold text-slate-300 transition-colors hover:bg-slate-600 disabled:cursor-wait disabled:bg-slate-800"
      >
        {registerGuest.isPending ? "Creating guest..." : "Play as Guest"}
      </button>
    </>
  );
}
