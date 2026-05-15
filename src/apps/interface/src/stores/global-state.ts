import { Alert } from '@/types/alert';
import { create } from 'zustand';

interface GlobalState {
  alerts: Alert[];
  addAlert: (message: string, type: 'error' | 'warning' | 'info' | 'success') => void;
  removeAlert: (id: number) => void;
}

export const useGlobalState = create<GlobalState>((set) => ({
  alerts: [],
  addAlert: (message, type) =>
    set((state) => ({
      alerts: [...state.alerts, { message, type, id: Date.now() }], // Using timestamp as unique ID
    })),
  removeAlert: (id) =>
    set((state) => ({
      alerts: state.alerts.filter((alert) => alert.id !== id),
    })),
}));