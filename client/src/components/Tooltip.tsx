import { useCallback, useEffect, useMemo, useRef, useState, type ReactNode } from "react";
import { createPortal } from "react-dom";
import { tooltipBase } from "../styles/styles";

export function Tooltip({
  content,
  children,
}: {
  content: ReactNode;
  children: ReactNode;
}) {
  const [show, setShow] = useState(false);
  const anchorRef = useRef<HTMLSpanElement>(null);
  const tooltipRef = useRef<HTMLDivElement>(null);
  const [pos, setPos] = useState({ top: 0, left: 0 });
  const [flip, setFlip] = useState(false);
  const isMobile = useMemo(() => window.matchMedia("(pointer: coarse)").matches, []);

  useEffect(() => {
    if (!show || !anchorRef.current) return;
    const rect = anchorRef.current.getBoundingClientRect();
    const shouldFlip = rect.top < window.innerHeight - rect.bottom;
    setFlip(shouldFlip);
    setPos({
      top: shouldFlip ? rect.bottom + 8 : rect.top - 8,
      left: rect.left + rect.width / 2,
    });
  }, [show]);

  useEffect(() => {
    if (!show || !tooltipRef.current) return;
    const el = tooltipRef.current;
    const r = el.getBoundingClientRect();
    const PAD = 8;
    if (r.right > window.innerWidth - PAD)
      el.style.left = `${window.innerWidth - PAD - r.width / 2}px`;
    if (r.left < PAD) el.style.left = `${PAD + r.width / 2}px`;
  }, [show, pos]);

  const toggle = useCallback(() => setShow((v) => !v), []);

  useEffect(() => {
    if (!isMobile || !show) return;
    const onDocClick = (e: MouseEvent) => {
      if (anchorRef.current && !anchorRef.current.contains(e.target as Node)) {
        setShow(false);
      }
    };
    document.addEventListener("click", onDocClick);
    return () => document.removeEventListener("click", onDocClick);
  }, [isMobile, show]);

  const hoverProps = isMobile
    ? { onClick: (e: React.MouseEvent) => { if (!(e.target as HTMLElement).closest("button, input, select, a")) toggle(); } }
    : { onMouseEnter: () => setShow(true), onMouseLeave: () => setShow(false) };

  return (
    <span ref={anchorRef} {...hoverProps}>
      {children}
      {show &&
        createPortal(
          <div
            ref={tooltipRef}
            className={`pointer-events-none fixed z-50 w-max max-w-xs -translate-x-1/2 ${flip ? "translate-y-0 pt-1.5" : "-translate-y-full"} ${tooltipBase}`}
            style={{ top: pos.top, left: pos.left }}
          >
            {content}
          </div>,
          document.body,
        )}
    </span>
  );
}
