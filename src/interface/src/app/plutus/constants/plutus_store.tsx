import { create } from "zustand";
import { timeFrames } from "./time_frames";

export interface PlutusState {
    tab: number,
    timeFrameSeconds: number,
    setTab: (x: number) => void,
    setTimeFrameSeconds: (seconds: number) => void,
};

export const usePlutusStore = create<PlutusState>((set) => ({
    tab: 0,
    availableMarkets: [],
    timeFrameSeconds: timeFrames[0].seconds,
    setTab: (x: number) => set({ tab: x }),
    setTimeFrameSeconds: (seconds: number) => set({ timeFrameSeconds: seconds }),
}));
