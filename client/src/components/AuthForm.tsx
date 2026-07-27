import { useState, type FormEvent } from "react";
import { registerSchema, type RegisterForm, type FormErrors } from "../schemas/auth";
import { inputBase, inputDefault, inputError } from "../styles/styles";

interface Props {
  initialUsername?: string;
  submitLabel: string;
  isPending: boolean;
  error: string | null;
  onSubmit: (data: { username: string; email: string; password: string }) => void;
  children?: React.ReactNode;
}

export function AuthForm({ initialUsername, submitLabel, isPending, error, onSubmit, children }: Props) {
  const [email, setEmail] = useState("");
  const [username, setUsername] = useState(initialUsername ?? "");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [errors, setErrors] = useState<FormErrors<RegisterForm>>({});
  const [touched, setTouched] = useState<Partial<Record<keyof RegisterForm, boolean>>>({});

  const validate = (values: Partial<RegisterForm>): FormErrors<RegisterForm> => {
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
    const values = { email, username, password, confirmPassword, [field]: value };
    if (field === "email") setEmail(value);
    if (field === "username") setUsername(value);
    if (field === "password") setPassword(value);
    if (field === "confirmPassword") setConfirmPassword(value);
    if (touched[field]) setErrors(validate(values));
  };

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    const result = registerSchema.safeParse({ email, username, password, confirmPassword });
    if (!result.success) {
      setTouched({ email: true, username: true, password: true, confirmPassword: true });
      setErrors(validate({ email, username, password, confirmPassword }));
      return;
    }
    setErrors({});
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    const { confirmPassword: _, ...payload } = result.data;
    onSubmit(payload);
  };

  const field = (label: string, name: keyof RegisterForm, type: string) => (
    <label className="flex flex-col gap-1.5">
      <span className="text-sm text-slate-300">{label}</span>
      <input
        type={type}
        value={name === "email" ? email : name === "username" ? username : name === "password" ? password : confirmPassword}
        onChange={(e) => handleChange(name, e.target.value)}
        onBlur={() => handleBlur(name)}
        className={`${inputBase} ${errors[name] ? inputError : inputDefault}`}
      />
      {errors[name] && <span className="text-xs text-red-400">{errors[name]}</span>}
    </label>
  );

  return (
    <form onSubmit={handleSubmit} className="flex flex-col gap-4">
      {field("Username", "username", "text")}
      {field("Email", "email", "email")}
      {field("Password", "password", "password")}
      {field("Confirm password", "confirmPassword", "password")}

      {error && (
        <div className="rounded-md bg-red-950 px-3 py-2 text-sm text-red-400">{error}</div>
      )}

      {children ?? (
        <button
          type="submit"
          disabled={isPending}
          className="mt-2 cursor-pointer rounded-md bg-green-600 py-3 text-base font-semibold text-white transition-colors hover:bg-green-500 disabled:cursor-wait disabled:bg-slate-600"
        >
          {isPending ? "Saving..." : submitLabel}
        </button>
      )}
    </form>
  );
}
