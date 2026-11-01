"use client";

import { useApi } from "@/hooks/use-api";
import { hermesApi } from "@/lib/api/hermes";
import React, { createContext, useContext } from "react";

const PublicModeContext = createContext(false);

/** Fetches Hermes public mode once for every page below it. */
export function PublicModeProvider({ children }: React.PropsWithChildren) {
  const [state] = useApi(() => hermesApi.getPublicMode());

  // Unknown counts as on, like the gateway, so write controls never flash up for visitors.
  const isPublicMode = state.data?.isEnabled ?? true;

  return (
    <PublicModeContext.Provider value={isPublicMode}>
      {children}
    </PublicModeContext.Provider>
  );
}

/** Whether Hermes is read-only, so pages should hide create, edit, save and delete controls. */
export function usePublicMode(): boolean {
  return useContext(PublicModeContext);
}
