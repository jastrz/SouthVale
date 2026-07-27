import { Link, useNavigate } from "@tanstack/react-router";
import type { AxiosError } from "axios";
import { useRegister } from "../api/hooks/useAuth";
import { useAuthStore } from "../store/authStore";
import { AuthForm } from "../components/AuthForm";
import { GuestRegisterButton } from "../components/GuestRegisterButton";

export function RegisterPage() {
  const navigate = useNavigate();
  const setAuth = useAuthStore((s) => s.setAuth);
  const register = useRegister();

  const serverError =
    (register.error as AxiosError<{ detail?: string }> | null)?.response?.data?.detail ??
    register.error?.message ??
    null;

  return (
    <div className="flex w-[90vw] max-w-96 flex-col gap-4 rounded-xl bg-slate-800 p-8 shadow-2xl">
      <h1 className="mb-2 text-center text-2xl font-semibold text-white">Register</h1>

      <AuthForm
        submitLabel="Register"
        isPending={register.isPending}
        error={serverError}
        onSubmit={(data) => {
          register.mutate(data, {
            onSuccess: (response) => {
              setAuth(response.data.accessToken, data.email, data.username);
              navigate({ to: "/" });
            },
          });
        }}
      />

      <div className="text-center text-sm text-slate-400">
        Already have an account?{" "}
        <Link to="/login" className="text-blue-400 underline hover:text-blue-300">Login</Link>
      </div>

      <GuestRegisterButton />
    </div>
  );
}
