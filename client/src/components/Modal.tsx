import { useEffect, useRef, type CSSProperties, type ReactNode } from "react";

export function Modal({ open, onClose, style, children, closeOnBackdrop = true }: { open: boolean; onClose: () => void; style?: CSSProperties; children: ReactNode; closeOnBackdrop?: boolean }) {
  const ref = useRef<HTMLDialogElement>(null);
  const openedAt = useRef(0);

  useEffect(() => {
    const d = ref.current;
    if (!d) return;
    if (open) { d.showModal(); openedAt.current = Date.now(); }
    else d.close();
  }, [open]);

  return (
    <dialog
      ref={ref}
      onClose={onClose}
      onClick={(e) => {
        if (e.target !== e.currentTarget) return;
        if (!closeOnBackdrop) return;
        if (Date.now() - openedAt.current < 100) return;
        onClose();
      }}
      className="w-full max-w-md rounded-xl bg-slate-800 p-0 text-white backdrop:bg-black/20 shadow-2xl"
      style={style ?? { margin: "auto" }}
    >
      {children}
    </dialog>
  );
}
