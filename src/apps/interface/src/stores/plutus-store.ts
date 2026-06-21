import {
  type DataTableState,
  DEFAULT_FILTER_MODE,
  EMPTY_FILTER,
} from "@/components/shared/responsive-data-table";
import { TimeFrameKey } from "@/app/(plutus)/plutus/constants/time-frames";
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
    filter: EMPTY_FILTER,
    sort: { totalGain: "DESC" },
    filterMode: DEFAULT_FILTER_MODE,
    smartQuery: "",
  },
  setExplorerTableState: (state) => set({ explorerTableState: state }),
  recipesTableState: {
    pagination: { pageSize: 10, skip: 0, take: 10 },
    filter: EMPTY_FILTER,
    sort: { averageMargin: "DESC" },
    filterMode: DEFAULT_FILTER_MODE,
    smartQuery: "",
  },
  setRecipesTableState: (state) => set({ recipesTableState: state }),
  forecastsTableState: {
    pagination: { pageSize: 10, skip: 0, take: 10 },
    filter: EMPTY_FILTER,
    sort: { dayOne: { gain: "DESC" } },
    filterMode: DEFAULT_FILTER_MODE,
    smartQuery: "",
  },
  setForecastsTableState: (state) => set({ forecastsTableState: state }),
  recentTradesTableState: {
    pagination: { pageSize: 10, skip: 0, take: 10 },
    filter: EMPTY_FILTER,
    sort: {},
    filterMode: DEFAULT_FILTER_MODE,
    smartQuery: "",
  },
  setRecentTradesTableState: (state) => set({ recentTradesTableState: state }),
  signalRankingsTableState: {
    pagination: { pageSize: 10, skip: 0, take: 10 },
    filter: {
      logic: "and",
      items: [{ field: "signalCount", operator: "gt", value: 1 }],
    },
    sort: { overallScore: "DESC" },
    filterMode: DEFAULT_FILTER_MODE,
    smartQuery: "",
  },
  setSignalRankingsTableState: (state) =>
    set({ signalRankingsTableState: state }),
  symbolGroupsTableState: {
    pagination: { pageSize: 10, skip: 0, take: 10 },
    filter: EMPTY_FILTER,
    sort: { name: "ASC" },
    filterMode: DEFAULT_FILTER_MODE,
    smartQuery: "",
  },
  setSymbolGroupsTableState: (state) => set({ symbolGroupsTableState: state }),
  strategiesTableState: {
    pagination: { pageSize: 10, skip: 0, take: 10 },
    filter: EMPTY_FILTER,
    sort: { createdAt: "DESC" },
    filterMode: DEFAULT_FILTER_MODE,
    smartQuery: "",
  },
  setStrategiesTableState: (state) => set({ strategiesTableState: state }),
  backtestsTableState: {
    pagination: { pageSize: 10, skip: 0, take: 10 },
    filter: EMPTY_FILTER,
    sort: { createdAt: "DESC" },
    filterMode: DEFAULT_FILTER_MODE,
    smartQuery: "",
  },
  setBacktestsTableState: (state) => set({ backtestsTableState: state }),
  openPositionsTableState: {
    pagination: { pageSize: 10, skip: 0, take: 10 },
    filter: EMPTY_FILTER,
    sort: { createdAt: "DESC" },
    filterMode: DEFAULT_FILTER_MODE,
    smartQuery: "",
  },
  setOpenPositionsTableState: (state) =>
    set({ openPositionsTableState: state }),
  closedPositionsTableState: {
    pagination: { pageSize: 10, skip: 0, take: 10 },
    filter: EMPTY_FILTER,
    sort: { createdAt: "DESC" },
    filterMode: DEFAULT_FILTER_MODE,
    smartQuery: "",
  },
  setClosedPositionsTableState: (state) =>
    set({ closedPositionsTableState: state }),
}));
