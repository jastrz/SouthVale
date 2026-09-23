import { useEffect, useRef, useState, type CSSProperties, type PointerEvent, type ReactNode } from "react";

export function ScrollArea({ children, className = "", style }: { children: ReactNode; className?: string; style?: CSSProperties }) {
  const scrollRef = useRef<HTMLDivElement>(null);
  const contentRef = useRef<HTMLDivElement>(null);
  const [thumb, setThumb] = useState<{ top: number; height: number } | null>(null);
  const drag = useRef<{ y: number; scrollTop: number } | null>(null);

  useEffect(() => {
    const el = scrollRef.current;
    const content = contentRef.current;
    if (!el || !content) return;

    const update = () => {
      const { scrollTop, scrollHeight, clientHeight } = el;
      if (scrollHeight <= clientHeight) {
        setThumb(null);
        return;
      }
      const height = Math.max(24, (clientHeight / scrollHeight) * clientHeight);
      const top = (scrollTop / (scrollHeight - clientHeight)) * (clientHeight - height);
      setThumb({ top, height });
    };

    update();
    el.addEventListener("scroll", update, { passive: true });
    const observer = new ResizeObserver(update);
    observer.observe(el);
    observer.observe(content);
    return () => {
      el.removeEventListener("scroll", update);
      observer.disconnect();
    };
  }, []);

  const onPointerDown = (e: PointerEvent<HTMLDivElement>) => {
    const el = scrollRef.current;
    if (!el) return;
    e.currentTarget.setPointerCapture(e.pointerId);
    drag.current = { y: e.clientY, scrollTop: el.scrollTop };
  };

  const onPointerMove = (e: PointerEvent<HTMLDivElement>) => {
    const el = scrollRef.current;
    if (!el || !drag.current || !thumb) return;
    const range = el.scrollHeight - el.clientHeight;
    const thumbRange = el.clientHeight - thumb.height;
    el.scrollTop = drag.current.scrollTop + ((e.clientY - drag.current.y) * range) / thumbRange;
  };

  const endDrag = () => {
    drag.current = null;
  };

  return (
    <div className={`relative ${className}`} style={style}>
      <div ref={scrollRef} className="scrollbar-none h-full max-h-[inherit] overflow-y-auto">
        <div ref={contentRef}>{children}</div>
      </div>
      {thumb && (
        <div
          onPointerDown={onPointerDown}
          onPointerMove={onPointerMove}
          onPointerUp={endDrag}
          onPointerCancel={endDrag}
          className="absolute right-0.5 w-1.5 touch-none cursor-pointer rounded-full bg-slate-500/50 hover:bg-slate-400/70"
          style={{ top: thumb.top, height: thumb.height }}
        />
      )}
    </div>
  );
}
