import { useState, type FormEvent } from "react";
import { Link, useNavigate } from "@tanstack/react-router";
import type { AxiosError } from "axios";
import { useRegister } from "../api/hooks/useAuth";
import { useAuthStore } from "../store/authStore";
import {
  registerSchema,
  type RegisterForm,
  type FormErrors,
} from "../schemas/auth";
import { inputBase, inputDefault, inputError } from "../styles/inputs";

export function RegisterPage() {
  const navigate = useNavigate();
  const setAuth = useAuthStore((s) => s.setAuth);
  const register = useRegister();

  const [email, setEmail] = useState("");
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [errors, setErrors] = useState<FormErrors<RegisterForm>>({});
  const [touched, setTouched] = useState<
    Partial<Record<keyof RegisterForm, boolean>>
  >({});

  const validate = (
    values: Partial<RegisterForm>,
  ): FormErrors<RegisterForm> => {
    const result = registerSchema.safeParse(values);
    if (!result.success) {
      const fieldErrors: FormErrors<RegisterForm> = {};
      result.error.issues.forEach((issue) => {
        const field = issue.path[0] as keyof RegisterForm;
        fieldErrors[field] = issue.message;
      });
      return fieldErrors;
    }
    return {};
  };

  const handleBlur = (field: keyof RegisterForm) => {
    setTouched((t) => ({ ...t, [field]: true }));
    setErrors(validate({ email, username, password, confirmPassword }));
  };

  const handleChange = (field: keyof RegisterForm, value: string) => {
    const values = {
      email,
      username,
      password,
      confirmPassword,
      [field]: value,
    };
    if (field === "email") setEmail(value);
    if (field === "username") setUsername(value);
    if (field === "password") setPassword(value);
    if (field === "confirmPassword") setConfirmPassword(value);
    if (touched[field]) setErrors(validate(values));
  };

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    const result = registerSchema.safeParse({
      email,
      username,
      password,
      confirmPassword,
    });
    if (!result.success) {
      setTouched({
        email: true,
        username: true,
        password: true,
        confirmPassword: true,
      });
      setErrors(validate({ email, username, password, confirmPassword }));
      return;
    }
    setErrors({});
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    const { confirmPassword: _, ...payload } = result.data;
    register.mutate(payload, {
      onSuccess: (response) => {
        setAuth(response.data.accessToken, email, username);
        navigate({ to: "/" });
      },
    });
  };

  const serverError =
    (register.error as AxiosError<{ message?: string }> | null)?.response?.data
      ?.message ??
    register.error?.message ??
    "Registration failed";

  return (
    <div className="flex h-screen w-screen items-center justify-center bg-slate-950">
      <form
        onSubmit={handleSubmit}
        className="flex w-96 flex-col gap-4 rounded-xl bg-slate-800 p-8 shadow-xl"
      >
        <h1 className="mb-2 text-center text-2xl font-semibold text-white">
          Register
        </h1>

        <label className="flex flex-col gap-1.5">
          <span className="text-sm text-slate-300">Username</span>
          <input
            type="text"
            value={username}
            onChange={(e) => handleChange("username", e.target.value)}
            onBlur={() => handleBlur("username")}
            className={`${inputBase} ${errors.username ? inputError : inputDefault}`}
          />
          {errors.username && (
            <span className="text-xs text-red-400">{errors.username}</span>
          )}
        </label>

        <label className="flex flex-col gap-1.5">
          <span className="text-sm text-slate-300">Email</span>
          <input
            type="email"
            value={email}
            onChange={(e) => handleChange("email", e.target.value)}
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
            onChange={(e) => handleChange("password", e.target.value)}
            onBlur={() => handleBlur("password")}
            className={`${inputBase} ${errors.password ? inputError : inputDefault}`}
          />
          {errors.password && (
            <span className="text-xs text-red-400">{errors.password}</span>
          )}
        </label>

        <label className="flex flex-col gap-1.5">
          <span className="text-sm text-slate-300">Confirm password</span>
          <input
            type="password"
            value={confirmPassword}
            onChange={(e) => handleChange("confirmPassword", e.target.value)}
            onBlur={() => handleBlur("confirmPassword")}
            className={`${inputBase} ${errors.confirmPassword ? inputError : inputDefault}`}
          />
          {errors.confirmPassword && (
            <span className="text-xs text-red-400">
              {errors.confirmPassword}
            </span>
          )}
        </label>

        {register.isError && (
          <div className="rounded-md bg-red-950 px-3 py-2 text-sm text-red-400">
            {serverError}
          </div>
        )}

        <button
          type="submit"
          disabled={register.isPending}
          className="mt-2 cursor-pointer rounded-md bg-green-600 py-3 text-base font-semibold text-white transition-colors hover:bg-green-500 disabled:cursor-wait disabled:bg-slate-600"
        >
          {register.isPending ? "Creating account..." : "Register"}
        </button>

        <div className="text-center text-sm text-slate-400">
          Already have an account?{" "}
          <Link
            to="/login"
            className="text-blue-400 underline hover:text-blue-300"
          >
            Login
          </Link>
        </div>
      </form>
    </div>
  );
}
