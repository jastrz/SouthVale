type IconProps = { src: string; alt?: string; size?: number };

export function Icon({ src, alt = "", size = 16 }: IconProps) {
  return (
    <img
      src={src}
      alt={alt}
      style={{ width: size, height: size, display: "inline-block", verticalAlign: "middle" }}
      loading="lazy"
    />
  );
}
