import { useState, useRef } from "react";

export function VillageHeader({
  name,
  x,
  y,
  villageId,
  onRename,
}: {
  name: string;
  x: number;
  y: number;
  villageId: string;
  onRename: (villageId: string, name: string) => void;
}) {
  const [editing, setEditing] = useState(false);
  const [draft, setDraft] = useState(name);
  const inputRef = useRef<HTMLInputElement>(null);

  function startEditing() {
    setDraft(name);
    setEditing(true);
    requestAnimationFrame(() => {
      inputRef.current?.focus();
      inputRef.current?.select();
    });
  }

  function submit() {
    const trimmed = draft.trim();
    if (trimmed && trimmed !== name) {
      onRename(villageId, trimmed);
    }
    setEditing(false);
  }

  return (
    <div className="border-b border-slate-800 px-4 py-3">
      {editing ? (
        <input
          ref={inputRef}
          className="w-full bg-slate-700 px-1 text-sm font-bold text-white outline-none ring-1 ring-slate-500"
          value={draft}
          onChange={(e) => setDraft(e.target.value)}
          onBlur={submit}
          onKeyDown={(e) => {
            if (e.key === "Enter") submit();
            if (e.key === "Escape") setEditing(false);
          }}
        />
      ) : (
        <h2
          className="cursor-pointer text-sm font-bold text-white hover:text-slate-300"
          onClick={startEditing}
          title="Click to rename"
        >
          {name}
        </h2>
      )}
      <p className="text-xs text-slate-400">
        {x}, {y}
      </p>
    </div>
  );
}
