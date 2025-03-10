import { timeFrames } from "@/app/plutus/constants/time_frames";
import { create } from "zustand";

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
