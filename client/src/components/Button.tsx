import type { ButtonHTMLAttributes } from "react";

export function Button({
  className = "",
  hoverClassName = "hover:shadow-md hover:scale-101",
  activeClassName = "active:scale-95",
  ...props
}: ButtonHTMLAttributes<HTMLButtonElement> & {
  hoverClassName?: string;
  activeClassName?: string;
}) {
  return (
    <button
      {...props}
      className={`transition-all ${hoverClassName} ${activeClassName} ${className}`}
    />
  );
}
