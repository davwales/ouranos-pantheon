import { create } from "zustand";

type BreadcrumbState = {
  labels: Record<string, string>;
  setLabel: (segment: string, label: string) => void;
  clearLabel: (segment: string) => void;
};

export const useBreadcrumbStore = create<BreadcrumbState>((set) => ({
  labels: {},
  setLabel: (segment, label) =>
    set((state) => ({ labels: { ...state.labels, [segment]: label } })),
  clearLabel: (segment) =>
    set((state) => {
      const { [segment]: _removed, ...labels } = state.labels;
      return { labels };
    }),
}));
