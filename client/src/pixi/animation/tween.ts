export type Easing = (t: number) => number;

export const linear: Easing = (t) => t;

export const easeInOutCubic: Easing = (t) =>
  t < 0.5 ? 4 * t * t * t : 1 - Math.pow(-2 * t + 2, 3) / 2;

export type TweenOptions = {
  duration: number;
  ease?: Easing;
  onUpdate: (t: number) => void;
  onComplete?: () => void;
};

export type TweenHandle = () => void;

/**
 * Run a value tween over `duration` ms. The handle returned cancels the
 * tween mid-flight; the cancelled tween simply stops calling onUpdate.
 */
export function tween(opts: TweenOptions): TweenHandle {
  const start = performance.now();
  const ease = opts.ease ?? easeInOutCubic;
  let raf = 0;
  let cancelled = false;

  const step = (now: number): void => {
    if (cancelled) return;
    const t = Math.min(1, (now - start) / opts.duration);
    opts.onUpdate(ease(t));
    if (t < 1) {
      raf = requestAnimationFrame(step);
    } else {
      opts.onComplete?.();
    }
  };

  raf = requestAnimationFrame(step);

  return () => {
    cancelled = true;
    cancelAnimationFrame(raf);
  };
}
