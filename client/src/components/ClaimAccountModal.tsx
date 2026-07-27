import type { AxiosError } from "axios";
import { toast } from "sonner";
import { useClaimGuest } from "../api/hooks/useAuth";
import { useAuthStore } from "../store/authStore";
import { AuthForm } from "./AuthForm";

interface Props {
  onClose: () => void;
}

export function ClaimAccountModal({ onClose }: Props) {
  const setAuth = useAuthStore((s) => s.setAuth);
  const currentUsername = useAuthStore((s) => s.username);
  const claimGuest = useClaimGuest();

  const claimError =
    (claimGuest.error as AxiosError<{ detail?: string }> | undefined)?.response?.data?.detail ??
    claimGuest.error?.message ??
    null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60">
      <div className="flex w-80 flex-col gap-4 rounded-xl bg-slate-800 p-6 shadow-2xl">
        <h2 className="text-lg font-semibold text-white">Claim Account</h2>
        <p className="text-sm text-slate-400">
          Set a username, email, and password to keep your account.
        </p>

        <AuthForm
          initialUsername={currentUsername ?? undefined}
          submitLabel="Claim"
          isPending={claimGuest.isPending}
          error={claimError}
          onSubmit={(data) => {
            claimGuest.mutate({ email: data.email, newPassword: data.password, username: data.username }, {
              onSuccess: () => {
                setAuth(useAuthStore.getState().token!, data.email, data.username);
                toast.success("Account claimed!");
                onClose();
              }
            });
          }}
        >
          <div className="flex gap-2">
            <button
              type="button"
              onClick={onClose}
              className="flex-1 cursor-pointer rounded-md bg-slate-700 py-2 text-sm text-slate-300 hover:bg-slate-600"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={claimGuest.isPending}
              className="flex-1 cursor-pointer rounded-md bg-blue-600 py-2 text-sm font-semibold text-white hover:bg-blue-500 disabled:cursor-wait disabled:bg-slate-600"
            >
              {claimGuest.isPending ? "Saving..." : "Claim"}
            </button>
          </div>
        </AuthForm>
      </div>
    </div>
  );
}
