let active = 0;
const listeners = new Set<(busy: boolean) => void>();
const emit = () => listeners.forEach((fn) => fn(active > 0));

export const requestBusy = {
  get: () => active > 0,
  subscribe: (fn: (busy: boolean) => void) => {
    listeners.add(fn);
    fn(active > 0);
    return () => listeners.delete(fn);
  },
  inc: () => {
    active++;
    emit();
  },
  dec: () => {
    active = Math.max(0, active - 1);
    emit();
  },
};
