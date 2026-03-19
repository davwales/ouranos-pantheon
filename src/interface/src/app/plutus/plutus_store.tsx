import { DataTableState } from "@/app/components/responsive-data-table";
import { timeFrames } from "@/app/plutus/constants/time_frames";
import { create } from "zustand";

export interface PlutusState {
  timeFrameSeconds: number;
  setTimeFrameSeconds: (seconds: number) => void;
  explorerTableState: DataTableState;
  setExplorerTableState: (state: DataTableState) => void;
  recipesTableState: DataTableState;
  setRecipesTableState: (state: DataTableState) => void;
  forecastsTableState: DataTableState;
  setForecastsTableState: (state: DataTableState) => void;
  recentTradesTableState: DataTableState;
  setRecentTradesTableState: (state: DataTableState) => void;
}

export const usePlutusStore = create<PlutusState>((set) => ({
  timeFrameSeconds: timeFrames[0].seconds,
  setTimeFrameSeconds: (seconds) => set({ timeFrameSeconds: seconds }),
  explorerTableState: {
    pagination: { pageSize: 10, skip: 0, take: 10 },
    filter: {},
    sort: { totalGain: "DESC" },
  },
  setExplorerTableState: (state) => set({ explorerTableState: state }),
  recipesTableState: {
    pagination: { pageSize: 10, skip: 0, take: 10 },
    filter: {},
    sort: { averageMargin: "DESC" },
  },
  setRecipesTableState: (state) => set({ recipesTableState: state }),
  forecastsTableState: {
    pagination: { pageSize: 10, skip: 0, take: 10 },
    filter: {},
    sort: { dayOne: { gain: "DESC" } },
  },
  setForecastsTableState: (state) => set({ forecastsTableState: state }),
  recentTradesTableState: {
    pagination: { pageSize: 10, skip: 0, take: 10 },
    filter: {},
    sort: {},
  },
  setRecentTradesTableState: (state) => set({ recentTradesTableState: state }),
}));
