export function Spinner({ size = 14, className = "" }: { size?: number; className?: string }) {
  return (
    <span
      className={`inline-block animate-spin rounded-full border-2 border-white/40 border-t-white ${className}`}
      style={{ width: size, height: size }}
    />
  );
}
