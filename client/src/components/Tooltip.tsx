import { useEffect, useRef, useState, type ReactNode } from "react";
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
  const ref = useRef<HTMLSpanElement>(null);
  const [pos, setPos] = useState({ top: 0, left: 0 });
  const [flip, setFlip] = useState(false);

  useEffect(() => {
    if (!show || !ref.current) return;
    const rect = ref.current.getBoundingClientRect();
    const shouldFlip = rect.top < window.innerHeight - rect.bottom;
    setFlip(shouldFlip);
    setPos({
      top: shouldFlip ? rect.bottom + 8 : rect.top - 8,
      left: rect.left + rect.width / 2,
    });
  }, [show]);

  return (
    <span
      ref={ref}
      onMouseEnter={() => setShow(true)}
      onMouseLeave={() => setShow(false)}
    >
      {children}
      {show &&
        createPortal(
          <div
            className={`pointer-events-none fixed z-50 -translate-x-1/2 ${flip ? "translate-y-0 pt-1.5" : "-translate-y-full"} ${tooltipBase}`}
            style={{ top: pos.top, left: pos.left }}
          >
            {content}
          </div>,
          document.body,
        )}
    </span>
  );
}
