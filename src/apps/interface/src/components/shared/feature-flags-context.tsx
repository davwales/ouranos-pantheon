"use client";

import { useApi } from "@/hooks/use-api";
import { featureFlagsApi } from "@/lib/api/feature-flags";
import React, { createContext, useContext } from "react";

// Undefined while the flags are unknown (loading or failed). Without a provider every flag is off.
const FeatureFlagsContext = createContext<Record<string, boolean> | undefined>(
  {},
);

/** Fetches the gateway's feature flags once for the whole app. */
export function FeatureFlagsProvider({ children }: React.PropsWithChildren) {
  const [state] = useApi(() => featureFlagsApi.getAll());

  return (
    <FeatureFlagsContext.Provider value={state.data?.flags}>
      {children}
    </FeatureFlagsContext.Provider>
  );
}

/**
 * Whether a feature flag is enabled. A flag the gateway does not report is off, as it is on the
 * gateway; `whileUnknown` applies until the flags have loaded, so a flag that hides controls can
 * fail closed.
 */
export function useFeatureFlag(
  name: string,
  { whileUnknown = false }: { whileUnknown?: boolean } = {},
): boolean {
  const flags = useContext(FeatureFlagsContext);
  return flags ? (flags[name] ?? false) : whileUnknown;
}
