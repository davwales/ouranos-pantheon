import { DataTableState } from "@/app/components/responsive-data-table";
import { TimeFrameKey } from "@/app/plutus/constants/time_frames";
import { create } from "zustand";

export interface PlutusState {
  timeFrameKey: TimeFrameKey;
  setTimeFrameKey: (key: TimeFrameKey) => void;
  explorerTableState: DataTableState;
  setExplorerTableState: (state: DataTableState) => void;
  recipesTableState: DataTableState;
  setRecipesTableState: (state: DataTableState) => void;
  forecastsTableState: DataTableState;
  setForecastsTableState: (state: DataTableState) => void;
  recentTradesTableState: DataTableState;
  setRecentTradesTableState: (state: DataTableState) => void;
  signalRankingsTableState: DataTableState;
  setSignalRankingsTableState: (state: DataTableState) => void;
  symbolGroupsTableState: DataTableState;
  setSymbolGroupsTableState: (state: DataTableState) => void;
  strategiesTableState: DataTableState;
  setStrategiesTableState: (state: DataTableState) => void;
  backtestsTableState: DataTableState;
  setBacktestsTableState: (state: DataTableState) => void;
  openPositionsTableState: DataTableState;
  setOpenPositionsTableState: (state: DataTableState) => void;
  closedPositionsTableState: DataTableState;
  setClosedPositionsTableState: (state: DataTableState) => void;
}

export const usePlutusStore = create<PlutusState>((set) => ({
  timeFrameKey: "OneHour",
  setTimeFrameKey: (key) => set({ timeFrameKey: key }),
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
  signalRankingsTableState: {
    pagination: { pageSize: 10, skip: 0, take: 10 },
    filter: { signalCount: { gt: 1 } },
    sort: { overallScore: "DESC" },
  },
  setSignalRankingsTableState: (state) =>
    set({ signalRankingsTableState: state }),
  symbolGroupsTableState: {
    pagination: { pageSize: 10, skip: 0, take: 10 },
    filter: {},
    sort: { name: "ASC" },
  },
  setSymbolGroupsTableState: (state) => set({ symbolGroupsTableState: state }),
  strategiesTableState: {
    pagination: { pageSize: 10, skip: 0, take: 10 },
    filter: {},
    sort: { createdAt: "DESC" },
  },
  setStrategiesTableState: (state) => set({ strategiesTableState: state }),
  backtestsTableState: {
    pagination: { pageSize: 10, skip: 0, take: 10 },
    filter: {},
    sort: { createdAt: "DESC" },
  },
  setBacktestsTableState: (state) => set({ backtestsTableState: state }),
  openPositionsTableState: {
    pagination: { pageSize: 10, skip: 0, take: 10 },
    filter: {},
    sort: { createdAt: "DESC" },
  },
  setOpenPositionsTableState: (state) => set({ openPositionsTableState: state }),
  closedPositionsTableState: {
    pagination: { pageSize: 10, skip: 0, take: 10 },
    filter: {},
    sort: { createdAt: "DESC" },
  },
  setClosedPositionsTableState: (state) =>
    set({ closedPositionsTableState: state }),
}));
