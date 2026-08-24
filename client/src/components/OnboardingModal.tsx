import { useState } from "react";
import { Modal } from "./Modal";
import { Icon } from "./Icon";
import { useOnboardingOpen } from "../store/onboardingStore";
import { useGameConfig } from "../api/hooks/useQueries";
import { BUILDING_ICONS, TROOP_ICONS, RESOURCE_ICONS, UI_ICONS } from "../lib/helpers";
import type { GameConfigDto } from "../api/types";

const ONBOARDING_KEY = "onboardingSeen";

function useOnboardingSeen() {
  const [seen, setSeen] = useState(() => {
    try {
      return localStorage.getItem(ONBOARDING_KEY) === "1";
    } catch {
      return false;
    }
  });
  const markSeen = () => {
    try {
      localStorage.setItem(ONBOARDING_KEY, "1");
    } catch {
      /* ignore */
    }
    setSeen(true);
  };
  return { seen, markSeen };
}

const STEPS: {
  title: string;
  body: React.ReactNode | ((config: GameConfigDto | undefined) => React.ReactNode);
}[] = [
  {
    title: "👋 Welcome, mayor!",
    body: (
      <>
        <p>
          Build resource buildings, train an army and raid your neighbors.
          Everything runs in real time — production and travel keep going
          while you're away.
        </p>
        <p>
          The <Icon src={UI_ICONS.info} size={16} /> <b>Info</b> tab above has
          the full reference for buildings and troops.
        </p>
      </>
    ),
  },
  {
    title: "🔨 Build",
    body: (
      <>
        <p className="flex flex-wrap items-center gap-1">
          <Icon src={BUILDING_ICONS.WoodCutter} size={16} /> <b>WoodCutter</b>,{" "}
          <Icon src={BUILDING_ICONS.ClayPit} size={16} /> <b>ClayPit</b>,{" "}
          <Icon src={BUILDING_ICONS.IronMine} size={16} /> <b>IronMine</b> and{" "}
          <Icon src={BUILDING_ICONS.Brewery} size={16} /> <b>Brewery</b>{" "}
          produce <Icon src={RESOURCE_ICONS.wood} size={16} /> <b>Wood</b>,{" "}
          <Icon src={RESOURCE_ICONS.clay} size={16} /> <b>Clay</b>,{" "}
          <Icon src={RESOURCE_ICONS.iron} size={16} /> <b>Iron</b> and{" "}
          <Icon src={RESOURCE_ICONS.beer} size={16} /> <b>Beer</b> every hour.
        </p>
        <p>
          Upgrade the <Icon src={BUILDING_ICONS.TownHall} size={16} />{" "}
          <b>TownHall</b> to build faster and the{" "}
          <Icon src={BUILDING_ICONS.Warehouse} size={16} /> <b>Warehouse</b>{" "}
          to store more — production above the storage cap is lost.{" "}
          <Icon src={BUILDING_ICONS.Barracks} size={16} /> <b>Barracks</b> and{" "}<br/>
          <Icon src={BUILDING_ICONS.Stable} size={16} /> <b>Stable</b> unlock
          troops.
        </p>
      </>
    ),
  },
  {
    title: "🗡️ Train",
    body: (
      <>
        <p className="flex flex-wrap items-center gap-1">
          <Icon src={BUILDING_ICONS.Barracks} size={16} /> <b>Barracks</b>{" "}
          trains infantry (
          <Icon src={TROOP_ICONS.Swordsman} size={16} /> <b>Swordsmen</b>,{" "}
          <Icon src={TROOP_ICONS.Archer} size={16} /> <b>Archers</b>,{" "}
          <Icon src={TROOP_ICONS.Dogs} size={16} /> <b>Dogs</b>), the{" "}
          <Icon src={BUILDING_ICONS.Stable} size={16} /> <b>Stable</b> trains
          cavalry (
          <Icon src={TROOP_ICONS.Horsemen} size={16} /> <b>Horsemen</b>,{" "}
          <Icon src={TROOP_ICONS.LlamaRiders} size={16} />{" "}
          <b>Llama Riders</b>).
        </p>
        <p>
          Training costs resources. Every troop drinks{" "}
          <Icon src={RESOURCE_ICONS.beer} size={16} /> <b>Beer</b> per hour —
          if the <Icon src={BUILDING_ICONS.Brewery} size={16} />{" "}
          <b>brewery</b> can't keep up, troops <b>starve</b>.
        </p>
      </>
    ),
  },
  {
    title: "🌍 Expand & Trade",
    body: (config) => (
      <>
        <p>
          Found new villages: train a{" "}
          <Icon src={TROOP_ICONS.Settler} size={16} /> <b>Settler</b>, click
          an empty grass tile on the map and send them out — up to{" "}
          {config?.maxVillagesPerPlayer ?? "your village cap"} villages in
          your empire. <b>Settler</b> training cost doubles with each village.
        </p>
        <p>
          The <Icon src={BUILDING_ICONS.TradePost} size={16} />{" "}
          <b>TradePost</b> converts resources into others (higher level =
          better rate), and <b>Transport</b> ships resources between your
          villages in real time.
        </p>
      </>
    ),
  },
  {
    title: "⚔️ Attack",
    body: (
      <>
        <p>
          Select an enemy village on the map and send your army. Attack power
          comes from your troops plus{" "}
          <Icon src={BUILDING_ICONS.Barracks} size={16} /> <b>Barracks</b>/
          <Icon src={BUILDING_ICONS.Stable} size={16} /> <b>Stable</b> bonuses;
          the target's <Icon src={BUILDING_ICONS.Wall} size={16} />{" "}
          <b>Wall</b> boosts its defense.
        </p>
        <p>
          Loot is limited by your carry capacity, and the target's{" "}
          <Icon src={BUILDING_ICONS.Cranny} size={16} /> <b>Cranny</b> hides
          resources from raiders. Survivors return home with the loot — read
          all about it in the <Icon src={UI_ICONS.notifications} size={16} />{" "}
          <b>Notifications</b> tab.
        </p>
      </>
    ),
  },
];

export function OnboardingModal() {
  const { seen, markSeen } = useOnboardingSeen();
  const forcedOpen = useOnboardingOpen((s) => s.open);
  const hideForced = useOnboardingOpen((s) => s.hide);
  const { data: config } = useGameConfig();
  const [step, setStep] = useState(0);

  const [prevOpen, setPrevOpen] = useState(forcedOpen);
  if (forcedOpen !== prevOpen) {
    setPrevOpen(forcedOpen);
    if (forcedOpen) setStep(0);
  }

  if (!forcedOpen && seen) return null;

  const last = step === STEPS.length - 1;
  const close = () => {
    markSeen();
    hideForced();
  };

  return (
    <Modal open onClose={close} closeOnBackdrop={false}>
      <div className="flex flex-col gap-3 p-4">
        <div className="flex items-center justify-between">
          <h2 key={step} className="step-anim text-base font-bold text-white">
            {STEPS[step].title}
          </h2>
          <span className="text-xs text-slate-400">
            {step + 1}/{STEPS.length}
          </span>
        </div>
        <div
          key={step}
          className="step-anim flex h-32 flex-col gap-2 overflow-y-auto text-xs leading-relaxed text-slate-300"
        >
          {typeof STEPS[step].body === "function"
            ? STEPS[step].body(config)
            : STEPS[step].body}
        </div>
        <div className="mt-2 flex items-center justify-between">
          {last ? (
            <span />
          ) : (
            <button
              onClick={close}
              className="rounded-lg bg-slate-700 px-3 py-1.5 text-xs font-bold text-slate-300 transition-all hover:bg-slate-600 hover:text-white hover:shadow-md active:scale-95"
            >
              Skip
            </button>
          )}
          <button
            onClick={() => (last ? close() : setStep((s) => s + 1))}
            className="rounded-lg bg-emerald-600 px-4 py-1.5 text-xs font-bold text-white transition-all hover:bg-emerald-500 hover:shadow-md active:scale-95 active:bg-emerald-700"
          >
            {last ? "Start playing" : "Next"}
          </button>
        </div>
      </div>
    </Modal>
  );
}
