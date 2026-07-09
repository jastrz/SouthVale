import { useState, useMemo } from "react";
import { useAddResources, useFullResources, useTickBarbarian, useTickLlm, useAdminVillages } from "../api/hooks/useAdmin";
import { LoginBar } from "../components/LoginBar";

function Btn({ label, loading, ...props }: {
  label: string;
  loading?: boolean;
} & React.ComponentPropsWithoutRef<"button">) {
  return (
    <button
      {...props}
      disabled={loading || props.disabled}
      className="cursor-pointer rounded bg-slate-700 px-3 py-2 text-sm font-medium text-slate-200 hover:bg-slate-600 disabled:opacity-50"
    >
      {loading ? `${label}...` : label}
    </button>
  );
}

export function AdminPage() {
  const [search, setSearch] = useState("");
  const [open, setOpen] = useState(false);
  const [villageId, setVillageId] = useState("");
  const [wood, setWood] = useState("1000");
  const [clay, setClay] = useState("1000");
  const [iron, setIron] = useState("1000");
  const [crop, setCrop] = useState("1000");

  const { data: villages } = useAdminVillages();
  const addResources = useAddResources();
  const fullResources = useFullResources();
  const tickBarbarian = useTickBarbarian();
  const tickLlm = useTickLlm();

  const filtered = useMemo(
    () => (villages ?? []).filter((v) => v.name.toLowerCase().includes(search.toLowerCase())),
    [villages, search],
  );

  const selected = villages?.find((v) => v.id === villageId);

  const handleAdd = (e: React.FormEvent) => {
    e.preventDefault();
    if (!villageId) return;
    addResources.mutate({
      villageId,
      resources: {
        wood: Number(wood),
        clay: Number(clay),
        iron: Number(iron),
        crop: Number(crop),
      },
    });
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-slate-900">
      <div className="flex w-full max-w-md flex-col gap-8">
        <div className="flex items-center justify-between">
          <h1 className="text-2xl font-bold text-white">Admin</h1>
          <LoginBar />
        </div>

        <form
          onSubmit={handleAdd}
          className="flex flex-col gap-3 rounded border border-slate-700 bg-slate-800 p-4"
        >
          <h2 className="text-sm font-semibold text-slate-300 uppercase">
            Add resources to village
          </h2>

          <input
            value={search}
            onChange={(e) => { setSearch(e.target.value); setVillageId(""); setOpen(true); }}
            onFocus={() => setOpen(true)}
            placeholder="Type village name..."
            className="rounded bg-slate-700 px-3 py-2 text-sm text-white placeholder-slate-500"
          />

          {open && search && filtered.length > 0 && (
            <div className="max-h-40 overflow-y-auto rounded border border-slate-600 bg-slate-800">
              {filtered.map((v) => (
                <button
                  key={v.id}
                  type="button"
                  onClick={() => { setVillageId(v.id); setSearch(v.name); setOpen(false); }}
                  className={`w-full cursor-pointer px-3 py-2 text-left text-sm transition-colors hover:bg-slate-600 ${
                    v.id === villageId ? "bg-slate-600 text-white" : "text-slate-300"
                  }`}
                >
                  {v.name} ({v.coordinates.x},{v.coordinates.y})
                </button>
              ))}
            </div>
          )}

          {selected && <p className="text-xs text-slate-500">ID: {selected.id}</p>}

          <div className="grid grid-cols-2 gap-2">
            {(["wood", "clay", "iron", "crop"] as const).map((r) => (
              <label key={r} className="flex flex-col gap-1 text-xs text-slate-400">
                {r.charAt(0).toUpperCase() + r.slice(1)}
                <input
                  type="number"
                  value={{ wood, clay, iron, crop }[r]}
                  onChange={(e) => {
                    const setter = { wood: setWood, clay: setClay, iron: setIron, crop: setCrop }[r];
                    setter(e.target.value);
                  }}
                  className="rounded bg-slate-700 px-2 py-1 text-sm text-white"
                />
              </label>
            ))}
          </div>

          <Btn type="submit" loading={addResources.isPending} label="Add resources" className="w-full mt-2" />
        </form>

        <div className="flex flex-col gap-3 rounded border border-slate-700 bg-slate-800 p-4">
          <h2 className="text-sm font-semibold text-slate-300 uppercase">
            Actions
          </h2>
          <Btn onClick={() => fullResources.mutate()} loading={fullResources.isPending} label="Fill all villages" className="w-full" />
          <Btn onClick={() => tickBarbarian.mutate()} loading={tickBarbarian.isPending} label="Barbarian tick" className="w-full" />
          <Btn onClick={() => tickLlm.mutate()} loading={tickLlm.isPending} label="LLM tick" className="w-full" />
        </div>
      </div>
    </div>
  );
}
