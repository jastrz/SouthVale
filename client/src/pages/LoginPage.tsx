import { useState, type FormEvent } from "react";
import { Link, useNavigate } from "@tanstack/react-router";
import type { AxiosError } from "axios";
import { useLogin } from "../api/hooks/useAuth";
import { useAuthStore } from "../store/authStore";
import { loginSchema, type LoginForm, type FormErrors } from "../schemas/auth";
import { inputBase, inputDefault, inputError } from "../styles/inputs";

export function LoginPage() {
  const navigate = useNavigate();
  const setAuth = useAuthStore((s) => s.setAuth);
  const login = useLogin();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [errors, setErrors] = useState<FormErrors<LoginForm>>({});
  const [touched, setTouched] = useState<
    Partial<Record<keyof LoginForm, boolean>>
  >({});

  const validate = (values: Partial<LoginForm>): FormErrors<LoginForm> => {
    const result = loginSchema.safeParse(values);
    if (!result.success) {
      const fieldErrors: FormErrors<LoginForm> = {};
      result.error.issues.forEach((issue) => {
        const field = issue.path[0] as keyof LoginForm;
        fieldErrors[field] = issue.message;
      });
      return fieldErrors;
    }
    return {};
  };

  const handleBlur = (field: keyof LoginForm) => {
    setTouched((t) => ({ ...t, [field]: true }));
    setErrors(validate({ email, password }));
  };

  const handleEmailChange = (value: string) => {
    setEmail(value);
    if (touched.email) setErrors(validate({ email: value, password }));
  };

  const handlePasswordChange = (value: string) => {
    setPassword(value);
    if (touched.password) setErrors(validate({ email, password: value }));
  };

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    const result = loginSchema.safeParse({ email, password });
    if (!result.success) {
      setTouched({ email: true, password: true });
      setErrors(validate({ email, password }));
      return;
    }
    setErrors({});
    login.mutate(result.data, {
      onSuccess: (response) => {
        setAuth(response.data.accessToken, email, response.data.username);
        navigate({ to: "/" });
      },
    });
  };

  const serverError =
    (login.error as AxiosError<{ message?: string }> | null)?.response?.data
      ?.message ??
    login.error?.message ??
    "Login failed";

  return (
    <div className="flex h-screen w-screen items-center justify-center bg-slate-950">
      <form
        onSubmit={handleSubmit}
        className="flex w-96 flex-col gap-4 rounded-xl bg-slate-800 p-8 shadow-xl"
      >
        <h1 className="mb-2 text-center text-2xl font-semibold text-white">
          Login
        </h1>

        <label className="flex flex-col gap-1.5">
          <span className="text-sm text-slate-300">Email</span>
          <input
            type="email"
            value={email}
            onChange={(e) => handleEmailChange(e.target.value)}
            onBlur={() => handleBlur("email")}
            className={`${inputBase} ${errors.email ? inputError : inputDefault}`}
          />
          {errors.email && (
            <span className="text-xs text-red-400">{errors.email}</span>
          )}
        </label>

        <label className="flex flex-col gap-1.5">
          <span className="text-sm text-slate-300">Password</span>
          <input
            type="password"
            value={password}
            onChange={(e) => handlePasswordChange(e.target.value)}
            onBlur={() => handleBlur("password")}
            className={`${inputBase} ${errors.password ? inputError : inputDefault}`}
          />
          {errors.password && (
            <span className="text-xs text-red-400">{errors.password}</span>
          )}
        </label>

        {login.isError && (
          <div className="rounded-md bg-red-950 px-3 py-2 text-sm text-red-400">
            {serverError}
          </div>
        )}

        <button
          type="submit"
          disabled={login.isPending}
          className="mt-2 cursor-pointer rounded-md bg-blue-600 py-3 text-base font-semibold text-white transition-colors hover:bg-blue-500 disabled:cursor-wait disabled:bg-slate-600"
        >
          {login.isPending ? "Logging in..." : "Login"}
        </button>

        <div className="text-center text-sm text-slate-400">
          Don&apos;t have an account?{" "}
          <Link
            to="/register"
            className="text-green-400 underline hover:text-green-300"
          >
            Register
          </Link>
        </div>
      </form>
    </div>
  );
}
